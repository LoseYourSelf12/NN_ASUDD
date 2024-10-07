import torch
import cv2
import numpy as np
import datetime
import queue
import time

from deep_sort_realtime.deepsort_tracker import DeepSort
from queue import Queue
from threading import Thread, Event

from modules.config import LoadConfig
# from modules.events import Event
from modules.img_processing import img_processing

# Загрузка конфигурации
config = LoadConfig()

# Загрузка модели
model = torch.hub.load(config.yolo_path, 'custom', config.model_path, source='local')

# Загрузка трекера объектов
tracker = DeepSort(max_age=30, n_init=2, nms_max_overlap=1.0, max_cosine_distance=0.2)

# Загрузка видеопотока
cap1 = cv2.VideoCapture(config.config['test_vid'])  # "cam_ip" или "test_vid"
cap2 = cv2.VideoCapture(config.config['test_vid'])  # "cam_ip" или "test_vid"
cap3 = cv2.VideoCapture(config.config['test_vid'])  # "cam_ip" или "test_vid"
cap4 = cv2.VideoCapture(config.config['test_vid'])  # "cam_ip" или "test_vid"

# Изображение для колибровки
img_col = cv2.imread("asudneyro/test_vid/test_img.png", cv2.IMREAD_COLOR)
# Инициализация очереди фреймов
frame_queue = Queue(maxsize=config.config['queue_size'])

# Инициализация ивентов
DETECT_EVENT1 = Event()
DETECT_EVENT2 = Event()
DETECT_EVENT3 = Event()
DETECT_EVENT4 = Event()


STOP_EVENT = Event()
# Ивенты на показ для каждого потока
SHOW_EVENT = Event()
# SHOW_EVENT.set()
SHOW_EVENT1 = Event()
# SHOW_EVENT1.set()
SHOW_EVENT2 = Event()
# SHOW_EVENT2.set()
SHOW_EVENT3 = Event()
# SHOW_EVENT3.set()
SHOW_EVENT4 = Event()
# SHOW_EVENT4.set()


# Глобальные переменные
RES = None  # хранение результатов обработки
START_TIME = None
END_TIME = None

def plug():
    """
    Заглушка под будущие реализации
    """
    pass

class VideoProcessor:
    """
    Класс обработчика потоков.\n
    При инициализации принимает модель в переменную, далее работает с ней.
    Порядок обработки реализован через очередь - принимает словарь.
    """
    def __init__(self, model) -> None:
        self.model = model
        self.joint_queue = Queue()
        self.stop_event = Event()
        self.stop_event.clear()

    def detect(self):
        """
        Принимает на вход словарь, где:
        - ключ - информация о камере;
        - значение - список изображений для обработки.
        """
        while not self.stop_event.is_set():
            queue_success, cap_info, img_list = self.pars_queue()

            if not queue_success:
                continue

            res = self.model(img_list)
            print(res)
            
            print(f'Камера {cap_info} обработана')
            print(f'======================')
            print(f'Список изображений ({len(img_list)}) обработан')
            """ДОБАВИТЬ ЛОГИКУ
            - обработка совместной очереди
            - обработка списка изображений
            - обработка результатов
            - отправка результатов
            - ...
            """
    
    def stop_detect(self):
        """
        Останавливает поток детекции через ивент
        """
        self.stop_event.set()

    def add_cap_dict(self, cap_dict):
        """
        Добавляет словарь с изображениями в очередь на обработку 
        """
        self.joint_queue.put(cap_dict)

    def pars_queue(self):
        try:
            # Берем словарь из очереди
            current_dict = self.joint_queue.get(timeout=1)

            # Определяем инфо из ключа
            cap_info = list(current_dict.keys())[0]
            
            # По этому же ключу определяем список изображений
            img_list = current_dict[cap_info]

            print(f'Получен список из камеры {cap_info}')
            print(f'Количество изображений: {len(img_list)}')
            
            return True, cap_info, img_list
        except queue.Empty:
            return False, None, None
            
def img_selection(cap, event, id_nd, cap_name, thr_name):
    time.sleep(1)
    print(f'img_selection {cap_name}')
    res = [img_col]
    res_dict = {}
    count = 1
    while not STOP_EVENT.is_set():
        # Когда ивент неактивен первый раз за цикл - формируем словарь и отправляем 
        # и очищаем переменные
        if not event.is_set():
            res_dict[f'@RMC:{id_nd}|&|{cap_name}_{thr_name}_time|'] = res

            processor.add_cap_dict(res_dict)

            res_dict = {}

            res = []

        # Ждем включение ивента
        event.wait(timeout=100)

        success, img = cap.read()

        if not success:
            plug()
            continue
        img, new_state = img_processing(img, SHOW_EVENT.is_set())
        count += 1
        if count == 10:
            res.append(img)
            count = 1
    
    event.clear()
    cap.release()


def main():
    """
    Основной цикл обработки
    Запускает ивенты детекции/записи результатов и пр.
    Запускается в основном потоке
    """ 
    print('main')
    cycle_time = 20
    zone_time = 5

    while not cycle_time == 0:
        
        if cycle_time == 20:
            DETECT_EVENT1.set()
            print('@')
        if cycle_time == 15:
            DETECT_EVENT1.clear()
            DETECT_EVENT2.set()
            print('@')
        if cycle_time == 10:
            DETECT_EVENT2.clear()
            DETECT_EVENT3.set()
            print('@')
        if cycle_time == 5:
            DETECT_EVENT3.clear()
            DETECT_EVENT4.set()
        time.sleep(zone_time)
        cycle_time -= zone_time
    DETECT_EVENT4.clear()


    cv2.destroyAllWindows()
    STOP_EVENT.set()
    processor.stop_detect()




if __name__ == '__main__':
    processor = VideoProcessor(model=model)

    print('Запуск 1 потока')
    thread_1 = Thread(target=main)
    print('Запуск 2 потока')
    thread_2 = Thread(target=processor.detect, daemon=True)
    print('Запуск 3-6 потока')
    thread_cap1 = Thread(target=img_selection, args=(cap1, DETECT_EVENT1, 1, 'камера 1', 'поток 1'), daemon=True)
    thread_cap2 = Thread(target=img_selection, args=(cap2, DETECT_EVENT2, 1, 'камера 2', 'поток 2'), daemon=True)
    thread_cap3 = Thread(target=img_selection, args=(cap3, DETECT_EVENT3, 1, 'камера 3', 'поток 3'), daemon=True)
    thread_cap4 = Thread(target=img_selection, args=(cap4, DETECT_EVENT4, 1, 'камера 4', 'поток 4'), daemon=True)
    print('Запуск окончен')

    thread_1.start()
    thread_2.start()
    thread_cap1.start()
    thread_cap2.start()
    thread_cap3.start()
    thread_cap4.start()

    thread_1.join()
    thread_2.join()
    thread_cap1.join()
    thread_cap2.join()
    thread_cap3.join()
    thread_cap4.join()
    

