import logging
import time
import json

from asudneyro.config_loader import ConfigLoader
from asudneyro.camera_manager import CameraManager
from asudneyro.results_manager import LineCrossingCounter
from asudneyro.detector import DetectionModule, YOLOv5Detector, ObjectTracker

class TrafficCycleManager:
    def __init__(self, camera_config_path, cycle_config_path, detection_module, result_path):
        # Передаем два пути к конфигам в ConfigLoader
        self.config_loader = ConfigLoader(camera_config_path, cycle_config_path)
        self.detection_module = detection_module
        self.camera_manager = CameraManager(self.config_loader)
        self.result_path = result_path
        self.current_phase = None
        self.start_time = None
        self.cycle_start_time = None

    def start_cycle(self):
        # Загружаем данные о циклах и фазах
        phases = self.config_loader.get_cycle_phases()
        cycle_duration = self.config_loader.get_cycle_duration()
        self.cycle_start_time = time.time()

        while True:
            # self.start_time = time.time()

            # Проходим по всем фазам
            for phase in phases:
                self.start_time = time.time()
                self.current_phase = phase
                logging.info(f"Активная фаза: {self.current_phase['phase_id']}")

                # Получаем список активных камер для текущей фазы
                active_cameras = self.config_loader.get_active_cameras_in_phase(self.current_phase['phase_id'])

                # Включаем камеры для текущей фазы
                for camera_id in active_cameras:
                    self.camera_manager.start_camera(camera_id, self.detection_module)

                # Подсчет пересечений линий
                for camera_id in active_cameras:
                    # Получаем активные линии пересечения для этой камеры в текущей фазе
                    active_cross_lines = self.config_loader.get_active_cross_lines_in_phase(self.current_phase['phase_id'], camera_id)
                    # print(active_cross_lines)
                    if active_cross_lines:
                        line_counter = LineCrossingCounter(
                            lines=active_cross_lines,
                            phase_duration=phase['duration'],
                            cycle_duration=cycle_duration,
                            camera_id=camera_id
                        )
                        # Получаем трекнутые объекты и считаем пересечения
                        # tracked_objects = self.detection_module.process_batch()
                        # if tracked_objects:
                        #     line_counter.check_crossing(tracked_objects)

                # Ждем завершения фазы
                while time.time() - self.start_time < phase['duration']:
                    time.sleep(1)

                # Останавливаем камеры после завершения фазы
                # self.camera_manager.stop_all_cameras()
                for camera_id in active_cameras:
                    self.camera_manager.stop_camera(camera_id)

            # Проверяем завершение цикла
            if time.time() - self.cycle_start_time >= cycle_duration:
                self.save_and_reset()
                self.cycle_start_time = time.time()
    # TODO: Переработать логику сохзхранения результатов, а также их отправку
    def save_and_reset(self):
        """Сохраняет результаты и сбрасывает счетчики перед новым циклом."""
        # Получаем результаты для каждой камеры в текущей фазе
        results = {}
        active_cameras = self.config_loader.get_active_cameras_in_phase(self.current_phase['phase_id'])

        for camera_id in active_cameras:
            # Получаем активные линии пересечения для камеры
            active_cross_lines = self.config_loader.get_active_cross_lines_in_phase(self.current_phase['phase_id'], camera_id)
            if active_cross_lines:
                line_counter = LineCrossingCounter(
                    lines=active_cross_lines,
                    phase_duration=self.current_phase['duration'],
                    cycle_duration=self.config_loader.get_cycle_duration(),
                    camera_id=camera_id
                )
                results[camera_id] = line_counter.get_results()

        # Сохраняем результаты в файл
        with open(self.result_path, 'w') as f:
            json.dump(results, f)

        # Логируем результат
        logging.info(f"Результаты сохранены: {results}")

        # Сброс счетчиков пересечений
        for camera_id in active_cameras:
            line_counter.reset_counts()


if __name__ == "__main__":
    # Инициализируем трекер
    ot = ObjectTracker()
    # Инициализируем модель
    Yd = YOLOv5Detector()
    # Инициализируем модуль детекции
    dm = DetectionModule(Yd, ot)
    # Инициализируем обработчик цикла
    tcm = TrafficCycleManager("camera_config.json",
                              "traffic_cycle_config.json",
                              detection_module=dm,
                              result_path="result.json"
                              )
    
    tcm.start_cycle()