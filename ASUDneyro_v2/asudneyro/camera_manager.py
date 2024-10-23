import threading
import time
import logging

from .image_processor import ImageProcessor

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class CameraManager:
    def __init__(self, config_loader):
        self.config_loader = config_loader
        self.camera_threads = {}  # Словарь потоков для камер
        self.camera_events = {}   # События для управления камерами

    def start_camera(self, camera_id, detection_module):
        camera_config = self.config_loader.get_camera_config(camera_id)
        if camera_config:
            if camera_id in self.camera_threads and self.camera_threads[camera_id].is_alive():
                logging.warning(f"Камера {camera_id} уже запущена.")
                return
            
            # Инициализация процессора изображений
            processor = ImageProcessor(
                rtsp_stream=camera_config['rtsp_stream'],
                image_size=camera_config['image_size'],
                crop_polygon=camera_config['crop_polygon']
            )

            processor.detection_module = detection_module

            # Событие для управления захватом
            event = threading.Event()
            self.camera_events[camera_id] = event

            # Поток для работы с камерой
            camera_thread = threading.Thread(target=self._run_camera, args=(processor, event))
            self.camera_threads[camera_id] = camera_thread
            logging.info(f"Запуск камеры {camera_id}")
            camera_thread.start()

    def _run_camera(self, processor, event):
        self.processor = processor
        self.processor.start_capture()  # Включаем камеру

        # # Ожидаем команды на остановку
        # while not event.is_set():
        #     time.sleep(0.1)

        # processor.stop_capture()  # Остановка камеры

    def stop_camera(self, camera_id):
        if camera_id in self.camera_events:
            # self.camera_events[camera_id].set()  # Сигнал на остановку камеры
            self.processor.stop_capture()
            logging.info(f"Остановка камеры {camera_id}")
            

    def stop_all_cameras(self):
        for camera_id in self.camera_events:
            self.stop_camera(camera_id)

