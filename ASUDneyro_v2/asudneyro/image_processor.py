import cv2
import numpy as np
import logging
import time

# Настройка логирования
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')


class ImageProcessor:
    def __init__(self, rtsp_stream, image_size, crop_polygon):
        self.rtsp_stream = rtsp_stream
        self.image_size = tuple(image_size)
        self.crop_polygon = np.array(crop_polygon, dtype=np.int32)
        self.active = False  # Флаг для включения/выключения камеры
        self.detection_module = None

    def process_frame(self, frame):
        # Масштабируем изображение до нужного размера
        resized_frame = cv2.resize(frame, self.image_size)
        
        # Создаем маску по полигону
        mask = np.zeros_like(resized_frame)
        cv2.fillPoly(mask, [self.crop_polygon], (255, 255, 255))
        
        # Применяем маску
        masked_frame = cv2.bitwise_and(resized_frame, mask)
        
        # Заливка внешней области белым фоном
        white_background = np.full_like(resized_frame, (255, 255, 255))
        final_frame = np.where(mask == 0, white_background, masked_frame)
        
        return final_frame

    def capture_stream(self):
        logging.info(f"Запуск потока: {self.rtsp_stream}")
        cap = cv2.VideoCapture(self.rtsp_stream)
        if not cap.isOpened():
            logging.error(f"Ошибка открытия потока: {self.rtsp_stream}")
            return
        
        frame_count = 0  # Счетчик кадров

        while self.active:
            ret, frame = cap.read()
            if not ret:
                logging.warning(f"Проблема с получением кадра из {self.rtsp_stream}")
                break
            
            frame_count += 1

            if frame_count % 10 != 0:
                continue

            # Обработка кадра
            processed_frame = self.process_frame(frame)

            self.detection_module.add_frame(processed_frame)

            # Отображение результата (для отладки)
            cv2.imshow(f"Processed Frame - {self.rtsp_stream}", processed_frame)
            if cv2.waitKey(10) & 0xFF == ord('q'):
                logging.info("Остановка по запросу пользователя.")
                break

        cap.release()
        cv2.destroyAllWindows()

    def start_capture(self):
        """Включение камеры и захват потока."""
        self.active = True
        self.capture_stream()

    def stop_capture(self):
        """Выключение камеры."""
        logging.info(f"Остановка камеры: {self.rtsp_stream}")
        self.active = False
        time.sleep(0.5)
        cv2.destroyAllWindows()