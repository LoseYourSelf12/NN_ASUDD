import cv2
import json
import torch
import numpy as np
import datetime
import time
import asyncio
import websockets
from ultralytics import YOLO
from queue import Queue
from threading import Thread, Event

# Инициализируем модель(-и) и видеопоток(-и)
with open("configure/config.json", 'r', encoding='utf-8') as f:
    config = json.load(f)

torch.device(config["device"])

model = YOLO(config["model_path"])

cap = cv2.VideoCapture(config["test_vid"])  # "cam_ip" or "test_vid"

# Инициализируем маску внешнего полигона
mask = np.zeros((config["imgsz"], config["imgsz"]), dtype=np.uint8)
cv2.fillPoly(mask, [np.array(config["out_region"])], 255)
background = cv2.bitwise_not(mask)
background = cv2.cvtColor(background, cv2.COLOR_GRAY2BGR)

# Инициализируем очередь фреймов
frame_queue = Queue()  # Убираем ограничение на максимальный размер очереди
detect_event = Event()
fill_event = Event()

# Глобальная переменная для хранения результатов обработки кадров
res = None

def process_queue():
    result_buffer = {}
    count_queue = 0
    while not frame_queue.empty():
        frame = frame_queue.get_nowait()
        frame_boxes = process_frame(frame=frame)
        result_buffer[count_queue] = frame_boxes
        count_queue += 1
    asyncio.run(send_program_no())  # Отправляем номер программы после обработки очереди
    return result_buffer

def process_frame(frame):
    result = model.track(frame,
                         conf=config["conf"],
                         imgsz=config["imgsz"],
                         persist=True,
                         device=config["device"],
                         show=False)
    return result[0].boxes.xywh.tolist()

def detect_loop(stop_event):
    global frame_queue, detect_event, res

    while not stop_event.is_set():
        if not frame_queue.empty() and detect_event.is_set():
            print("Start detecting")
            res = process_queue()
            detect_event.clear()
            print("Detection finished.")

def process_and_write_results():
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
    pass

def main_loop(stop_event):
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
            frame_queue.put_nowait(im0)
            print("Added frame to queue.")
            detect_event.set()

        cv2.imshow("test", im0)
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    
    cap.release()
    cv2.destroyAllWindows()
    stop_event.set()  # Signal other threads to stop

async def send_program_no():
    global prog_no
    uri = "ws://localhost:8000/ws"  # Адрес вебсокета

    prog_no = (prog_no + 1) % 4  # Обновляем номер программы

    message = json.dumps({"progNo": prog_no})
    async with websockets.connect(uri) as websocket:
        await websocket.send(message)
        print(f"Sent program number: {prog_no}")

async def websocket_handler():
    uri = "ws://localhost:8000/ws"
    global fill_event

    async with websockets.connect(uri) as websocket:
        while True:
            message = await websocket.recv()
            data = json.loads(message)
            print(f"Received message: {data}")

            if 'taktNo' in data:
                taktNo = data['taktNo']
                print(f"taktNo: {taktNo}")

                if taktNo == 4:
                    fill_event.set()
                    print("Fill event set.")
                else:
                    fill_event.clear()
                    print("Fill event cleared.")

def websocket_thread():
    asyncio.run(websocket_handler())

prog_no = 0  # Инициализируем номер программы

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
stop_event.set()  # Ensure detect_loop stops if it hasn't already
t2.join()
t3.join()
t4.join()
