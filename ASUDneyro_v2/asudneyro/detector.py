import torch
import logging
import threading
from collections import deque
from deep_sort_realtime.deepsort_tracker import DeepSort

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

# Модель YOLOv5 с использованием TensorRT для оптимизации
class YOLOv5Detector:
    def __init__(self, conf_threshold=0.5):
        logging.info("Загрузка YOLOv5 модели с TensorRT оптимизацией")
        self.model = torch.hub.load('yolov5-master/', 'custom',
                                    'yolov5s.pt', source='local')
        # self.model = self.model.autoshape().cuda()  # Оптимизация через CUDA + TensorRT
        self.model.half()  # Используем полуточные вычисления
        self.conf_threshold = conf_threshold

    def detect(self, images):
        """Выполняем детекцию объектов на батче изображений"""
        batch_results = []
        for image in images:
            # Подготавливаем изображение
            img = torch.from_numpy(image).permute(2, 0, 1).unsqueeze(0).cuda().half()  # Подготовка под полуточные вычисления
            results = self.model(img)[0]
            # Применяем порог вероятности
            results = results[results[:, 4] > self.conf_threshold]
            
            # Преобразуем детекции в формат [x1, y1, x2, y2, score, class_id]
            processed_results = []
            for result in results:
                x1, y1, x2, y2, conf, class_id = result[:6].tolist()
                processed_results.append([(x1, y1, x2, y2), conf, class_id])
                # print("!!!!!!!!\n", x1, y1, x2, y2, conf, class_id)
            # print(processed_results)
            batch_results.append(processed_results)
        return batch_results

# Трекер с DeepSORT
class ObjectTracker:
    def __init__(self, max_age=5):
        self.tracker = DeepSort(max_age=max_age, n_init=2, nms_max_overlap=1.0, max_cosine_distance=0.2)

    def update(self, detections, frame):
        """Обновление треков на основе детекций"""
        return self.tracker.update_tracks(detections, frame=frame)

# Модуль для батчевой обработки с детекцией и трекингом
class DetectionModule:
    def __init__(self, detector, tracker, batch_size=5, max_frame_buffer=100):
        self.detector = detector
        self.tracker = tracker
        self.batch_size = batch_size
        self.frame_buffer = deque(maxlen=max_frame_buffer)
        self.active = True
        self.lock = threading.Lock()

    def add_frame(self, frame):
        """Добавляем кадр в буфер"""
        with self.lock:
            self.frame_buffer.append(frame)

    def process_batch(self):
        """Обработка батча кадров"""
        if len(self.frame_buffer) >= self.batch_size:
            with self.lock:
                batch = [self.frame_buffer.popleft() for _ in range(self.batch_size)]

            # Выполняем детекцию объектов на батче
            detections = self.detector.detect(batch)
            tracked_objects = []

            for i, frame in enumerate(batch):
                if detections[i]:
                    # Для каждого кадра запускаем трекинг
                    processed_detections = [((det[0][0], det[0][1], det[0][2], det[0][3]), det[1], det[2]) for det in detections[i]]
                    tracked = self.tracker.update(processed_detections, frame)
                else:
                    tracked = []

                
                tracked_objects.append(tracked)

            # Возвращаем трекнутые объекты для всех кадров в батче
            return tracked_objects

        return None

    def stop(self):
        """Остановка модуля"""
        self.active = False