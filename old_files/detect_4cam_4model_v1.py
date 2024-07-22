import cv2
import json
import time
import numpy as np
from ultralytics import YOLO

# Load the YOLO models
model1 = YOLO("s_860i_m50-0,67(0,75).pt")
model2 = YOLO("s_860i_m50-0,67(0,75).pt")
model3 = YOLO("s_860i_m50-0,67(0,75).pt")
model4 = YOLO("s_860i_m50-0,67(0,75).pt")

# Initialize video captures for 4 cameras
cap1 = cv2.VideoCapture("D:/for YOLO/Root111/domodedovo_test.mp4")
cap2 = cv2.VideoCapture("D:/for YOLO/Root111/out1.mp4")
cap3 = cv2.VideoCapture("D:/for YOLO/Root111/out2.mp4")
cap4 = cv2.VideoCapture("D:/for YOLO/Root111/domodedovo_test.mp4")

# Check if cameras are opened correctly
if not (cap1.isOpened() and cap2.isOpened() and cap3.isOpened() and cap4.isOpened()):
    print("Error: Could not open one or more video files.")
    exit()

classes_to_count = [0, 1, 2]  # Select classes to count
frame_tick = 1  # Frequency of frame capturing in seconds
tick = 15  # Frequency of writing to file in seconds

# For logging
count_1min = [[], [], [], []]
last_save_time = time.time()
last_frame_time = time.time()

def write_data(count_1min, tick):
    """Write detection results to JSON"""
    data = {
        "current_time": time.strftime("%Y-%m-%d %H:%M:%S"),
        "count_1min": {f"Region_{i+1}": len(count_1min[i]) for i in range(4)},
        "sum_1min": {f"Region_{i+1}": sum(count_1min[i]) for i in range(4)},
        "avg_1min": {f"Region_{i+1}": sum(count_1min[i]) / len(count_1min[i]) for i in range(4)}
    }
    # Convert all values to int
    for key in data["sum_1min"]:
        data["sum_1min"][key] = int(data["sum_1min"][key])
    for key in data["avg_1min"]:
        data["avg_1min"][key] = int(data["avg_1min"][key])

    with open("output.json", "w") as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

def process_frame(model, frame, classes_to_count, region_index):
    """Process a single frame using the given model"""
    results = model.track(source=frame, conf=0.6, imgsz=640, persist=True, device="cuda")
    boxes = results[0].boxes.xywh.cpu().numpy()
    class_ids = results[0].boxes.cls.cpu().numpy().astype(int)
    track_ids = results[0].boxes.id.cpu().numpy().astype(int)
    
    count = []
    for box, cls_id, tr_id in zip(boxes, class_ids, track_ids):
        if cls_id in classes_to_count:
            count.append(int(tr_id))  # Ensure track_id is an int
    
    return count

# Main loop
while cap1.isOpened() and cap2.isOpened() and cap3.isOpened() and cap4.isOpened():
    current_time = time.time()

    # Capture frames from each camera every second
    if current_time - last_frame_time >= frame_tick:
        ret1, frame1 = cap1.read()
        ret2, frame2 = cap2.read()
        ret3, frame3 = cap3.read()
        ret4, frame4 = cap4.read()

        if not (ret1 and ret2 and ret3 and ret4):
            break
        
        # Resize frames to 640x640
        frame1 = cv2.resize(frame1, (640, 640))
        frame2 = cv2.resize(frame2, (640, 640))
        frame3 = cv2.resize(frame3, (640, 640))
        frame4 = cv2.resize(frame4, (640, 640))

        # Process each frame separately
        count_1min[0].extend(process_frame(model1, frame1, classes_to_count, 0))
        count_1min[1].extend(process_frame(model2, frame2, classes_to_count, 1))
        count_1min[2].extend(process_frame(model3, frame3, classes_to_count, 2))
        count_1min[3].extend(process_frame(model4, frame4, classes_to_count, 3))

        last_frame_time = current_time

    # Write results to file at the specified frequency
    if current_time - last_save_time >= tick:
        write_data(count_1min, tick)
        last_save_time = time.time()
        count_1min = [[], [], [], []]

# Release video captures and close windows
cap1.release()
cap2.release()
cap3.release()
cap4.release()
cv2.destroyAllWindows()

