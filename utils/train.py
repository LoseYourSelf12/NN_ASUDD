from ultralytics import YOLO
import multiprocessing
import torch

torch.cuda.empty_cache()
model = YOLO('yolov8s')
    

if __name__ == '__main__':
    multiprocessing.freeze_support()  # Optional on Windows

    results = model.train(data='configure/dataset.yaml', epochs=300, imgsz=1280, batch=-1, name='yolov8s_pre')