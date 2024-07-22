import cv2
import torch
import numpy as np

from ultralytics import YOLO
from collections import defaultdict

torch.cuda.empty_cache()

model = YOLO('best_1300i_m50-0,81.pt')

video_path = "D:/for YOLO/Root111/domodedovo_test.mp4"
cap = cv2.VideoCapture(video_path)

track_history = defaultdict(lambda: [])
unique_ids_per_region = [set() for _ in range(3)]  # Список для хранения уникальных ID в каждой области

# Определение координат прямоугольных областей (замените на ваши реальные координаты)
regions = [
    (0.3, 0.5, 0.3, 0.3),  # Область 1
]
frame_counter = 0
# Loop through the video frames
while cap.isOpened():
    success, frame = cap.read()
    
    # Очищаем списки уникальных ID в начале каждого кадра
    unique_ids_per_region = [set() for _ in range(3)]

    if success:
        if frame_counter % 1 == 0:
            results = model.track(frame, conf=0.6, imgsz=1280, persist=True, device='cuda', vid_stride=1)

            boxes = results[0].boxes.xywh.cuda()
            track_ids = results[0].boxes.id.int().cuda().tolist()

            # Создаём копию фрейма для рисования результатов
            annotated_frame = frame.copy()

            for box, track_id in zip(boxes, track_ids):
                x, y, w, h = box
                # print(x, y, w, h)
                center_x, center_y = x + w / 2, y + h / 2

                # Проверяем, находится ли объект в одной из областей
            # Проверяем, находится ли объект в одной из областей
                for region_idx, (region_center_x, region_center_y, width, height) in enumerate(regions):
                    x1, y1 = region_center_x * frame.shape[1] - width / 2 * frame.shape[1], region_center_y * frame.shape[0] - height / 2 * frame.shape[0]
                    x2, y2 = region_center_x * frame.shape[1] + width / 2 * frame.shape[1], region_center_y * frame.shape[0] + height / 2 * frame.shape[0]

                    # Рисуем регион (для визуализации)
                    cv2.rectangle(annotated_frame, (int(x1), int(y1)), (int(x2), int(y2)), (0, 255, 0), 2)

                    center_x, center_y = box[0] + box[2] / 2, box[1] + box[3] / 2  # Центр bounding box в пикселях 

                    if x1 <= center_x <= x2 and y1 <= center_y <= y2:
                        # Добавляем ID в список уникальных ID для этой области
                        unique_ids_per_region[region_idx].add(track_id)

                        # Рисуем bounding box и трекер только для объектов в области
                        track = track_history[track_id]
                        track.append((float(x), float(y)))
                        if len(track) > 30:
                            track.pop(0)
                        points = np.hstack(track).astype(np.int32).reshape((-1, 1, 2))

                        x_top_left = int(x - w / 2)
                        y_top_left = int(y - h / 2)
                        # cv2.polylines(annotated_frame, [points], isClosed=False, color=(230, 230, 230), thickness=1)
                        cv2.rectangle(annotated_frame, (x_top_left, y_top_left), (int(x + w / 2), int(y + h / 2)), (255, 0, 0), 2)
                        cv2.putText(annotated_frame, f"ID: {track_id}", (int(x), int(y - 10)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)

            # Выводим количество уникальных ID в каждой области
            for region_idx, unique_ids in enumerate(unique_ids_per_region):
                cv2.putText(annotated_frame, f"Region {region_idx + 1}: {len(unique_ids)}", (10, 30 + region_idx * 20), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

            # Display the annotated frame
            cv2.imshow("YOLOv8 Inference", annotated_frame)

            if cv2.waitKey(1) & 0xFF == ord("q"):
                break
        frame_counter += 1
    else:
        break

cap.release()
cv2.destroyAllWindows()