import cv2
import json
import torch
import numpy as np
import datetime
import time
import websocket
from ultralytics import YOLO
from queue import Queue
from threading import Thread, Event

# Инициализируем модель(-и) и видеопоток(-и)
with open("configure/config.json", 'r', encoding='utf-8') as f:
    config = json.load(f)

# Выбор девайса - CUDA / CPU
torch.device(config["device"])

# Модель НС
model = YOLO(config["model_path"])

# Захват видео 
cap = cv2.VideoCapture(config["cam_ip"])  # "cam_ip" or "test_vid"

# Инициализируем маску внешнего полигона
mask = np.zeros((config["imgsz"], config["imgsz"]), dtype=np.uint8)
cv2.fillPoly(mask, [np.array(config["out_region"])], 255)
background = cv2.bitwise_not(mask)
background = cv2.cvtColor(background, cv2.COLOR_GRAY2BGR)

# Инициализируем очередь фреймов
frame_queue = Queue(maxsize=config["queue_size"])
detect_event = Event()
fill_event = Event()

# Глобальная переменная для хранения результатов обработки кадров
res = None

def process_queue():
    """
    Обрабатывает каждый кадр в очереди функцией process_frame.
    Записывает в буффер результатов для дальнейшего посчета.
    """
    result_buffer = {}
    count_queue = 0
    while not frame_queue.empty():
        frame = frame_queue.get_nowait()
        frame_boxes = process_frame(frame=frame)
        result_buffer[count_queue] = frame_boxes
        count_queue += 1
    return result_buffer

def process_frame(frame):
    """
    Обрабатывает кходной кадр, возвращает координат BB.
    Параметры в configure/config.json
    """
    result = model.track(frame,
                         conf=config["conf"],
                         imgsz=config["imgsz"],
                         persist=True,
                         device=config["device"],
                         show=False)
    return result[0].boxes.xywh.tolist()

def detect_loop(stop_event):
    """
    Цикл проверки для запуска потока детекции.
    Проверяет состояние очереди и состояние ивента.
    """
    global frame_queue, detect_event, res

    while not stop_event.is_set():
        if not frame_queue.empty() and detect_event.is_set():
            print("Start detecting")
            res = process_queue()
            detect_event.clear()
            continue

def process_and_write_results():
    """
    Записывает резуьтаты, если имеются
    """
    global res
    while True:
        if res:
            print("Writing boxes data...")
            with open("boxes.json", 'w', encoding='utf-8') as f:
                json.dump(res, f)
            print("Done\nWriting result data...")
            if config["counting_type"] == "out":
                out_poly(res)
            elif config["counting_type"] == "in":
                pass
            print("Done")
            res = {}
            
        time.sleep(1)

def out_poly(results):
    """
    Простой подсчет среднего количества объектов во внешнем полигоне
    """
    total_boxes = 0
    mean_boxes = 0
    for key in results.keys():
        total_boxes += len(results[key])
    mean_boxes = total_boxes / len(results)

    res = {"total_boxes": total_boxes,
           "mean_boxes": mean_boxes
           }
    with open("results.json", "w", encoding="utf-8") as f:
        json.dump(res, f)

def in_poly(results):
    """
    Подсчет во внутренних полигонах
    """
    pass

def main_loop(stop_event):
    """
    Основной цикл
    Считывает кадры с потока/видео
    Запускает ивенты для детекции/наполнения очереди
    Наполняет очередь
    """
    global frame_queue, detect_event, fill_event
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

        if fill_event.is_set():
            if frame_queue.full():
                detect_event.set()
            if not frame_queue.full() and not detect_event.is_set():
                frame_queue.put_nowait(im0)

        cv2.imshow("test", im0)
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    
    cap.release()
    cv2.destroyAllWindows()
    stop_event.set()  # Signal other threads to stop !!! не работает(

def on_message(ws, message):
    """
    Обратная связь с вебсокетом
    """
    global fill_event
    data = json.loads(message)
    print(f"Received message: {data}")

    if 'taktNo' in data:
        taktNo = data['taktNo']
        print(f"taktNo: {taktNo}")

        if taktNo == 14:
            fill_event.set()
        else:
            fill_event.clear()

def websocket_thread():
    ws = websocket.WebSocketApp("ws://194.87.111.128:1031",
                                on_message=on_message)
    ws.run_forever()

stop_event = Event()
t1 = Thread(target=main_loop, args=(stop_event,))
t2 = Thread(target=detect_loop, args=(stop_event,), daemon=True)
t3 = Thread(target=process_and_write_results, daemon=True)
t4 = Thread(target=websocket_thread, daemon=True)
t1.start()
t2.start()
t3.start()
t4.start()

t1.join()
stop_event.set()
t2.join()
t3.join()
t4.join()
