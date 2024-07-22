import cv2
import json
import time
import numpy as np
from ultralytics import YOLO

# Load the YOLO model
model = YOLO("s_860i_m50-0,67(0,75).pt")

# Initialize video captures for 4 cameras
cap1 = cv2.VideoCapture("D:/for YOLO/Root111/domodedovo_test.mp4")
cap2 = cv2.VideoCapture("D:/for YOLO/Root111/out1.mp4")
cap3 = cv2.VideoCapture("D:/for YOLO/Root111/out2.mp4")
cap4 = cv2.VideoCapture("D:/for YOLO/Root111/domodedovo_test.mp4")

# Check if cameras are opened correctly
if not (cap1.isOpened() and cap2.isOpened() and cap3.isOpened() and cap4.isOpened()):
    print("Error: Could not open one or more video files.")
    exit()

# Define regions for the 4 camera setup
regions = [
    np.array([(0, 0), (640, 0), (640, 640), (0, 640)]),           # Top-left
    np.array([(640, 0), (1280, 0), (1280, 640), (640, 640)]),     # Top-right
    np.array([(0, 640), (640, 640), (640, 1280), (0, 1280)]),     # Bottom-left
    np.array([(640, 640), (1280, 640), (1280, 1280), (640, 1280)]) # Bottom-right
]

classes_to_count = [0, 1, 2]  # Select classes to count
tick = 10  # Frequency of writing to file

# For logging
count = [[], [], [], []]
count_1min = [[], [], [], []]
last_save_time = time.time()

def write_data(count, tick):
    """Write detection results to JSON"""
    avg_fps = sum(len(c) for c in count) / (tick * len(regions))
    data = {
        "current_time": time.strftime("%Y-%m-%d %H:%M:%S"),
        "count_1min": {f"Region_{i+1}": len(count[i]) for i in range(len(regions))},
        "sum_1min": {f"Region_{i+1}": sum(count[i]) for i in range(len(regions))},
        "avg_1min": {f"Region_{i+1}": sum(count_1min[i]) / tick for i in range(len(regions))},
        "avg_fps": avg_fps
    }
    with open("output.json", "w") as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

def combine_images(images): 
    top_row = np.hstack(images[:2])
    bottom_row = np.hstack(images[2:])
    combined_image = np.vstack((top_row, bottom_row))
    return combined_image

def draw_boxes(image, boxes, class_ids, track_ids):
    for box, cls_id, tr_id in zip(boxes, class_ids, track_ids):
        x, y, w, h = box
        x1, y1 = int(x - w / 2), int(y - h / 2)
        x2, y2 = int(x + w / 2), int(y + h / 2)
        color = (0, 255, 0)  # Green for bounding box
        cv2.rectangle(image, (x1, y1), (x2, y2), color, 2)
        cv2.putText(image, f'ID: {tr_id}', (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)
        cv2.putText(image, f'Class: {cls_id}', (x1, y2 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)
    for region in regions:
        cv2.polylines(image, [region], isClosed=True, color=(0, 0, 255), thickness=2)  # Red for region polygons
    return image

# Main loop
while cap1.isOpened() and cap2.isOpened() and cap3.isOpened() and cap4.isOpened():
    start_time = time.time()

    # Capture frames from each camera
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

    # Combine frames into one image
    combined_image = combine_images([frame1, frame2, frame3, frame4])

    # Perform detection and tracking on the combined image
    results = model.track(source=combined_image, conf=0.6, imgsz=1280, persist=True, device="cuda")

    boxes = results[0].boxes.xywh.cpu().numpy()
    class_ids = results[0].boxes.cls.cpu().numpy().astype(int)
    track_ids = results[0].boxes.id.cpu().numpy().astype(int)

    combined_image = draw_boxes(combined_image, boxes, class_ids, track_ids)

    # Logging results
    for i, reg in enumerate(regions):
        count[i] = []
        for box, cls_id, tr_id in zip(boxes, class_ids, track_ids):
            x, y, w, h = box
            c1, c2 = x + w / 2, y + h / 2
            if cv2.pointPolygonTest(reg, (c1, c2), False) >= 0 and cls_id in classes_to_count:
                count[i].append([cls_id, tr_id])
    
        count_1min[i].append(len(count[i]))

    # Write results to file at the specified frequency
    if start_time - last_save_time > tick:
        write_data(count_1min, tick)
        last_save_time = time.time()
        count_1min = [[], [], [], []]

    cv2.imshow('Combined Image with Detections', combined_image)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release video captures and close windows
cap1.release()
cap2.release()
cap3.release()
cap4.release()
cv2.destroyAllWindows()
