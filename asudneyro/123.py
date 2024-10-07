import threading
import time
import cv2
import torch
import numpy as np
from queue import Queue
from deep_sort_realtime.deepsort_tracker import DeepSort

# Загрузка модели YOLOv5 (можешь подставить свою обученную модель)
model = torch.hub.load('ultralytics/yolov5', 'yolov5s', pretrained=True)

# Очередь для изображений
image_queue = Queue()

# Инициализация трекеров DeepSort для каждой камеры
trackers = [DeepSort(max_age=30) for _ in range(4)]  # Настроим max_age и другие параметры по необходимости

# Функция для захвата кадров с камер
def capture_camera(camera_id, path, image_queue):
    cap = cv2.VideoCapture(path)
    while True:
        ret, frame = cap.read()
        if not ret:
            break

        # Эмулируем команду детекции
        event_detect = True  # В реальной системе здесь будет запрос от сервера
        if event_detect:
            image_queue.put((camera_id, frame))
        
        time.sleep(0.1)  # Сон, чтобы снизить нагрузку

# Функция для детекции объектов и трекинга
def detection_thread():
    while True:
        if not image_queue.empty():
            camera_id, frame = image_queue.get()

            # YOLOv5 детекция
            results = model(frame)
            detections = results.xywh[0].cpu().numpy()  # Формат [x_center, y_center, width, height, confidence, class]

            if len(detections) > 0:
                bboxes = detections[:, :4]  # [x_center, y_center, width, height]
                confs = detections[:, 4]    # Уверенность в детекции
                classes = detections[:, 5]  # Классы объектов

                # Преобразуем координаты YOLO (центр) в формат [x1, y1, x2, y2]
                bbox_xyxy = np.zeros_like(bboxes)
                bbox_xyxy[:, 0] = bboxes[:, 0] - bboxes[:, 2] / 2  # x1
                bbox_xyxy[:, 1] = bboxes[:, 1] - bboxes[:, 3] / 2  # y1
                bbox_xyxy[:, 2] = bboxes[:, 0] + bboxes[:, 2] / 2  # x2
                bbox_xyxy[:, 3] = bboxes[:, 1] + bboxes[:, 3] / 2  # y2

                # Проверка форматов перед вызовом update_tracks
                print(f"camera_id: {camera_id}, bbox_xyxy: {bbox_xyxy}, confs: {confs}")

                # Убедимся, что мы передаем непустые и корректные данные
                if len(bbox_xyxy) > 0 and len(confs) > 0:
                    # Преобразуем массив уверенности в список
                    confs = confs.tolist()  # Преобразуем в список для корректной передачи
                    
                    # Трекинг: передаем bbox_xyxy и confs
                    try:
                        tracks = trackers[camera_id].update_tracks(bbox_xyxy, confs, frame=frame)  # Обновление треков

                        # Отображение объектов и ID на кадре
                        for track in tracks:
                            if not track.is_confirmed():
                                continue
                            track_id = track.track_id
                            ltrb = track.to_ltrb()  # Координаты: [left, top, right, bottom]

                            # Рисуем bbox и ID на кадре
                            cv2.rectangle(frame, (int(ltrb[0]), int(ltrb[1])), (int(ltrb[2]), int(ltrb[3])), (255, 0, 0), 2)
                            cv2.putText(frame, f'ID: {track_id}', (int(ltrb[0]), int(ltrb[1])-10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (255, 0, 0), 2)

                    except Exception as e:
                        print(f"Error in update_tracks for camera {camera_id}: {e}")
                else:
                    print(f"Skipping empty detections for camera {camera_id}")

            # Выводим кадр с детекцией
            cv2.imshow(f'Camera {camera_id}', frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        time.sleep(0.1)  # Небольшая задержка

# Запуск потоков для каждой камеры
camera_threads = []
for i in range(4):
    t = threading.Thread(target=capture_camera, args=(i, 'asudneyro/test_vid/test_vid_2.mp4', image_queue))
    t.start()
    camera_threads.append(t)

# Запуск потока для детекции
det_thread = threading.Thread(target=detection_thread)
det_thread.start()

# Ожидание завершения всех потоков
for t in camera_threads:
    t.join()
det_thread.join()

cv2.destroyAllWindows()
