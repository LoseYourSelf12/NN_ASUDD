import cv2
import json
import torch
import asyncio
import datetime
import numpy as np
from ultralytics import YOLO
from queue import Queue
from threading import Thread


# Инициализируем модель(-и) и видеопоток(-и)
with open("config.json", 'r', encoding='utf-8') as f:
    config = json.load(f)

torch.device(config["device"])

model = YOLO(config["model_path"])

cap = cv2.VideoCapture(config["cam_ip"]) # "cam_ip" or "test_vid"

# Инициализируем маску внешнего полигона
mask = np.zeros((config["imgsz"], config["imgsz"]), dtype=np.uint8)
cv2.fillPoly(mask, [np.array(config["out_region"])], 255)
background = cv2.bitwise_not(mask)
background = cv2.cvtColor(background, cv2.COLOR_GRAY2BGR)

# Инициализируем очередь фреймов
frame_queue = Queue()

async def process_queue():
    result_buffer = []
    count_queue = 0
    while not frame_queue.empty():
        frame = frame_queue.get()
        frame_boxes = process_frame(frame=frame)

        result_buffer.append(frame_boxes)
        count_queue += 1
    print("!!!Cuont of queue: ", count_queue)
    return result_buffer


def process_frame(frame):
    result = model.track(frame,
                         conf=config["conf"],
                         imgsz=config["imgsz"],
                         persist=True,
                         device=config["device"],
                         show=True)
    print("Frame processed!")
    return result[0].boxes




test_tick = 0
while cap.isOpened():
    success, im0 = cap.read()

    if not success:
        break  #??? add log

    im0 = cv2.resize(im0, (config["imgsz"], config["imgsz"]))
    im0 = cv2.bitwise_and(im0, im0, mask=mask)
    im0 = cv2.add(im0, background)

    test_tick += 1
    if test_tick < 30:
        frame_queue.put(im0)
    if test_tick == 30:
        res = asyncio.run(process_queue())
    print("test_tick: ", test_tick, datetime.datetime.now())

    cv2.imshow("test", im0)
    if cv2.waitKey(1) & 0xFF == ord("q"):
        break
    
cap.release()
cv2.destroyAllWindows()
