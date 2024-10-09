import cv2
import numpy as np
import threading
import queue
import time
import logging
from modules.config import LoadConfig

class VideoProcessor:
    def __init__(self, video_source=0, camera_id=0, show_event_state=False):
        """
        Инициализация класса обработки видео
        :param video_source: источник видеопотока (камера или видеофайл)
        :param show_event_state: флаг отображения событий (True/False)
        """
        self.config = LoadConfig()
        self.video_source = video_source
        self.camera_id = camera_id
        self.show_event_state = show_event_state
        self.running = False
        
        # Добавляем событие для детекции
        self.detection_event = threading.Event()

        # Очередь кадров для передачи между потоками
        self.frame_queue = queue.Queue(maxsize=1000)

        # Маска и фон для предобработки
        self.mask = np.zeros((self.config.config['imgsz'], self.config.config['imgsz']), dtype=np.uint8)
        cv2.fillPoly(self.mask, [np.array(self.config.config['out_region'])], 255)
        self.background = cv2.bitwise_not(self.mask)
        self.background = cv2.cvtColor(self.background, cv2.COLOR_GRAY2BGR)

        # Настройка логирования
        logging.basicConfig(level=logging.INFO)
        self.logger = logging.getLogger('VideoProcessor')

    def img_processing(self, img_in):
        """
        Предобработка изображения:
        - изменение размера
        - наложение маски
        - отрисовка полигонов и линий
        """
        # Изменение размера
        img_out = cv2.resize(img_in, (self.config.config['imgsz'], self.config.config['imgsz']))

        # Наложение маски
        img_out = cv2.bitwise_and(img_out, img_out, mask=self.mask)

        # Изменение фона
        img_out = cv2.add(img_out, self.background)

        # Отрисовка полигонов и линий, если включен SHOW_EVENT_STATE
        if self.show_event_state:
            # Внешний полигон
            cv2.polylines(img_out, [np.array(self.config.config['out_region'])], isClosed=True, color=(0, 255, 0), thickness=2)

            # Внутренние полигоны
            for in_reg in self.config.config['in_regions']:
                cv2.polylines(img_out, [np.array(in_reg)], isClosed=True, color=(255, 0, 0), thickness=2)

            # Линии пересечения
            for in_line in self.config.config['lines_with_directions']:
                cv2.polylines(img_out, [np.array(in_line['line'])], isClosed=True, color=(0, 0, 255), thickness=2)

            # Отображение изображения
            cv2.imshow('Debug Images', img_out)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                self.show_event_state = False
                cv2.destroyAllWindows()

        return img_out

    def video_capture(self):
        """
        Захват видеопотока и передача кадров на обработку.
        """
        self.logger.info(f"Запуск захвата видео. {self.camera_id}")
        cap = cv2.VideoCapture(self.video_source)
        if not cap.isOpened():
            self.logger.error(f"Не удалось открыть видеоисточник {self.video_source} {self.camera_id}")
            return
        
        while self.running:
            ret, frame = cap.read()
            if not ret:
                self.logger.warning(f"Не удалось считать кадр, остановка. {self.camera_id}")
                break

            # Помещаем кадр в очередь для дальнейшей обработки
            if not self.frame_queue.full():
                self.frame_queue.put(frame)
            else:
                self.logger.warning(f"Очередь кадров переполнена! {self.camera_id}")

            time.sleep(0.03)  # Ограничение FPS (опционально)

        cap.release()
        self.logger.info(f"Завершение захвата видео. {self.camera_id}")

    def process_frames(self):
        """
        Обработка кадров из очереди: предобработка и отрисовка.
        Реакция на событие детекции.
        """
        self.logger.info(f"Запуск обработки кадров. {self.camera_id}")
        while self.running:
            if not self.frame_queue.empty():
                frame = self.frame_queue.get()

                # Проверяем, нужно ли выполнять детекцию
                if self.detection_event.is_set():
                    self.logger.info(f"Детекция активирована. Обрабатываем кадры. {self.camera_id}")
                    processed_frame = self.img_processing(frame)
                else:
                    self.logger.info(f"Детекция неактивна. Пропускаем кадры. {self.camera_id}")
                    time.sleep(0.49)  # Ожидаем запуск детекции

                if self.show_event_state and self.detection_event.is_set():
                    cv2.imshow(f'Processed Frame - {self.camera_id}', processed_frame)

            time.sleep(0.01)  # Для разгрузки процессора
        self.logger.info(f"Завершение обработки кадров. {self.camera_id}")

    def start(self):
        """
        Запуск потоков захвата видео и обработки кадров.
        """
        self.running = True
        self.capture_thread = threading.Thread(target=self.video_capture, daemon=True)
        self.processing_thread = threading.Thread(target=self.process_frames, daemon=True)

        self.capture_thread.start()
        self.processing_thread.start()

        self.logger.info(f"Потоки захвата видео и обработки кадров запущены. {self.camera_id}")

    def stop(self):
        """
        Остановка всех потоков обработки и захвата видео.
        """
        self.running = False
        self.capture_thread.join()
        self.processing_thread.join()

        # Закрываем все окна OpenCV
        cv2.destroyAllWindows()

        self.logger.info(f"Потоки успешно завершены. {self.camera_id}")
