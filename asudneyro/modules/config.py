import json


class LoadConfig:
    def __init__(self):
        self.path = 'asudneyro/config/config.json'

        with open(self.path, 'r', encoding='utf-8') as f:
            self.config = json.load(f)  # полный конфиг файл
            self.model_path = self.config['model_path']
            self.yolo_path = 'asudneyro/models/yolov5-master/'
