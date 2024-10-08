import cv2
import json
import torch
import numpy as np
import datetime
import time
from queue import Queue
from threading import Thread, Event
from deep_sort_realtime.deepsort_tracker import DeepSort

# from traffic_sender import send_traffic_data

# Загрузка конфигурации
with open("configure/config.json", 'r', encoding='utf-8') as f:
    config = json.load(f)

device = torch.device(config["device"])

# Загрузка модели YOLOv5
model = torch.hub.load('asudneyro/models/yolov5-master/', 'custom', 'asudneyro/models/best_2000i_m50-0,87(0,93).pt', source='local')

# Инициализация DeepSORT
tracker = DeepSort(max_age=30, n_init=2, nms_max_overlap=1.0, max_cosine_distance=0.2)

cap = cv2.VideoCapture(config["test_vid"])  # "cam_ip" or "test_vid"

# Инициализация маски внешнего полигона
mask = np.zeros((config["imgsz"], config["imgsz"]), dtype=np.uint8)
cv2.fillPoly(mask, [np.array(config["out_region"])], 255)
background = cv2.bitwise_not(mask)
background = cv2.cvtColor(background, cv2.COLOR_GRAY2BGR)

# Инициализация очереди фреймов
frame_queue = Queue(maxsize=config["queue_size"])
detect_event = Event()

# Глобальная переменная для хранения результатов обработки кадров
res = None
# Глобальные переменные для храннеия времени
start_time = None
end_time = None

# Линии направлений (из конфигурации)
# directions = config["directions"]

# Функция для проверки пересечения направления
def check_direction_crossing(previous_position, current_position, direction_line):
    x1, y1 = previous_position
    x2, y2 = current_position
    line_start, line_end = direction_line
    
    # Проверка пересечения линии направления
    return cv2.lineTest(np.array([line_start, line_end]), (x1, y1), (x2, y2))

def process_queue():
    result_buffer = {}
    count_queue = 0
    while not frame_queue.empty():
        frame = frame_queue.get_nowait()
        frame_boxes = process_frame(frame=frame)
        result_buffer[count_queue] = frame_boxes
        count_queue += 1
    return result_buffer

def process_frame(frame):
    # Получаем предсказания YOLOv5
    results = model(frame)

    # Извлекаем боксы
    detections = []
    for *xyxy, conf, cls in results.xyxy[0]:
        if conf >= config["conf"]:
            x1, y1, x2, y2 = map(int, xyxy)
            bbox = [x1, y1, x2 - x1, y2 - y1]
            detections.append(bbox)
    
    # Трекинг объектов с использованием DeepSORT
    tracked_objects = tracker.update_tracks(detections, frame=frame)
    tracked_bboxes = []
    
    for track in tracked_objects:
        if not track.is_confirmed():
            continue
        track_id = track.track_id
        bbox = track.to_ltwh()
        tracked_bboxes.append((track_id, bbox))

    return tracked_bboxes

def detect_loop(stop_event):
    global frame_queue, detect_event, res, start_time, end_time

    while not stop_event.is_set():
        if not frame_queue.empty() and detect_event.is_set():
            print("Start detecting")
            end_time = datetime.datetime.now()
            res = process_queue()
            detect_event.clear()
            continue

def line_intersection(p1, p2, box_center):
    """
    Проверяем пересечение центра bbox с линией (p1, p2)
    """
    x1, y1 = p1
    x2, y2 = p2
    x, y = box_center
    return ((y - y1) * (x2 - x1)) == ((y2 - y1) * (x - x1))

def count_directions(tracked_bboxes, previous_coords, lines):
    """
    Функция для подсчета машин по направлениям
    """
    directions = {}

    for track_id, bbox in tracked_bboxes:
        x, y, w, h = bbox
        center = (x + w // 2, y + h // 2)

        # Получаем предыдущие координаты объекта
        prev_center = previous_coords.get(track_id)

        # Если есть предыдущие координаты, проверяем пересечения
        if prev_center:
            for direction, line in lines.items():
                if line_intersection(line[0], line[1], center) and not line_intersection(line[0], line[1], prev_center):
                    directions[direction] += 1

        # Обновляем координаты для объекта
        previous_coords[track_id] = center

    return directions

class Event:
    def __init__(self, event_name="", event_type="", event_duration=0, event_state=False):
        self.name = event_name
        self.type = event_type  
        self.duration = event_duration
        self.state = event_state
    
    def is_set(self):
        return self.state
    
    def set(self):
        self.state = True
    
    def clear(self):
        self.state = False

def process_and_write_results(stop_event):
    global res
    previous_coords = {}  # Для хранения предыдущих координат объектов

    # Определяем линии для подсчета направлений
    lines = {}
    # lines = {
    #     'straight': [(100, 200), (300, 400)],  # Заменить на реальные координаты линий
    #     'left': [(400, 500), (600, 700)],
    #     'right': [(200, 300), (500, 600)]
    # }
    for line_num, line in enumerate(config['lines_with_directions'], start=1):
        lines[f'direction_{line_num}'] = line['line']

    while not stop_event.is_set():
        if res:
            print("Processing results...")

            # Подсчет направлений движения
            print(res)
            directions = count_directions(res, previous_coords, lines)

            # Вывод результатов
            print("Направления:", directions)

            # Отправка данных на сервер
            send_traffic_data(straight=directions['straight'], left=directions['left'], right=directions['right'])

            # Сброс результатов
            res = {}
            
        time.sleep(1)

def point_in_polygon(point, polygon):
    x, y = point
    inside = False
    for i in range(len(polygon)):
        x1, y1 = polygon[i]
        x2, y2 = polygon[(i + 1) % len(polygon)]
        if y1 > y != y2 > y and x < (x2 - x1) * (y - y1) / (y2 - y1) + x1:
            inside = not inside
    return inside

def out_poly(results, start_time, end_time):
    total_boxes = 0
    for frame_boxes in results.values():
        total_boxes += len(frame_boxes)
    mean_boxes = total_boxes / len(results)

    # Пример данных для отправки
    straight_percentage = 70
    left_percentage = 20
    right_percentage = 10

    # Отправляем данные на сервер
    send_traffic_data(straight_percentage, left_percentage, right_percentage)

    res = {
        "start_time": start_time.isoformat(),
        "end_time": end_time.isoformat(),
        "total_boxes": total_boxes,
        "mean_boxes": mean_boxes
    }
    with open("results/results.json", "w", encoding="utf-8") as f:
        json.dump(res, f)

def in_poly(results, start_time, end_time):
    in_region_stats = {i: {"total_boxes": 0, "frames": []} for i in range(len(config["in_regions"]))}

    for frame_boxes in results.values():
        for i, in_region in enumerate(config["in_regions"]):
            count_in_region = sum(1 for box in frame_boxes if point_in_polygon((box[0] + box[2] / 2, box[1] + box[3] / 2), in_region))
            in_region_stats[i]["total_boxes"] += count_in_region
            in_region_stats[i]["frames"].append(count_in_region)

    for stats in in_region_stats.values():
        stats["mean_boxes"] = stats["total_boxes"] / len(stats["frames"])
        stats["min_boxes"] = min(stats["frames"])
        stats["max_boxes"] = max(stats["frames"])

    result = {
        "start_time": start_time.isoformat(),
        "end_time": end_time.isoformat(),
        "in_region_stats": in_region_stats
    }

    with open("results/in_region_results.json", "w", encoding="utf-8") as f:
        json.dump(result, f)

def draw_polygons(image):
    # Рисование внешнего полигона
    cv2.polylines(image, [np.array(config["out_region"])], isClosed=True, color=(0, 255, 0), thickness=2)
    # Рисование внутренних полигонов
    for in_region in config["in_regions"]:
        cv2.polylines(image, [np.array(in_region)], isClosed=True, color=(255, 0, 0), thickness=2)
    
    return image

def main_loop(stop_event):
    global frame_queue, detect_event, start_time
    test_tick = 0

    while cap.isOpened() and not stop_event.is_set():
        test_tick += 1
        print("test_tick: ", test_tick, datetime.datetime.now(), "thread: main")
        success, im0 = cap.read()

        if not success:
            print("Something's wrong!\nSkipped...")
            break

        im0 = cv2.resize(im0, (config["imgsz"], config["imgsz"]))
        im0 = cv2.bitwise_and(im0, im0, mask=mask)
        im0 = cv2.add(im0, background)
        im0 = draw_polygons(im0)

        if frame_queue.full():
            detect_event.set()
        if not frame_queue.full() and not detect_event.is_set():
            if frame_queue.qsize() == 0:
                start_time = datetime.datetime.now()
            frame_queue.put_nowait(im0)

        cv2.imshow("test", im0)
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    
    cap.release()
    cv2.destroyAllWindows()
    stop_event.set()  # Signal other threads to stop

stop_event = Event()
t1 = Thread(target=main_loop, args=(stop_event,))
t2 = Thread(target=detect_loop, args=(stop_event,), daemon=True)
t3 = Thread(target=process_and_write_results, args=(stop_event,), daemon=True)
t1.start()
t2.start()
t3.start()

t1.join()
stop_event.set()  # Ensure detect_loop stops if it hasn't already
t2.join()
t3.join()
