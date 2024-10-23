import logging
import json

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class ConfigLoader:
    def __init__(self, camera_config_path, cycle_config_path):
        self.camera_config_path = camera_config_path
        self.cycle_config_path = cycle_config_path
        self.camera_config = None
        self.cycle_config = None
        self._load_configs()

    def _load_configs(self):
        """Загружает конфигурационные файлы камер и циклов."""
        # Загружаем конфигурацию камер
        with open(self.camera_config_path, 'r') as f:
            self.camera_config = json.load(f)
        
        # Загружаем конфигурацию циклов
        with open(self.cycle_config_path, 'r') as f:
            self.cycle_config = json.load(f)

    def get_camera_config(self, camera_id):
        """Возвращает конфигурацию камеры по её ID."""
        for camera in self.camera_config['cameras']:
            if camera['camera_id'] == camera_id:
                return camera
        return None

    def get_all_cameras(self):
        """Возвращает все конфигурации камер."""
        return self.camera_config['cameras']

    def get_cycle_duration(self):
        """Возвращает общее время цикла."""
        return self.cycle_config['cycle_duration']

    def get_cycle_phases(self):
        """Возвращает список фаз циклов светофора."""
        return self.cycle_config['phases']

    def get_active_cameras_in_phase(self, phase_id):
        """Возвращает активные камеры для указанной фазы."""
        for phase in self.cycle_config['phases']:
            if phase['phase_id'] == phase_id:
                return phase['active_cameras']
        return []

    def get_active_cross_lines_in_phase(self, phase_id, camera_id):
        """Возвращает активные линии пересечения для камеры в указанной фазе."""
        active_lines = []
        
        # Получаем активные линии по названию для фазы
        for phase in self.cycle_config['phases']:
            if phase['phase_id'] == phase_id:
                active_line_names = phase['active_cross_lines'].get(str(camera_id), [])
        
        # Получаем конфигурацию камеры
        camera_config = self.get_camera_config(camera_id)
        
        if camera_config:
            for line in camera_config['cross_lines']:
                if line['name'] in active_line_names:
                    active_lines.append(line)
        
        return active_lines

    def get_cycle_report_interval(self):
        """Возвращает интервал отчетности в секундах."""
        return self.cycle_config['cycle_report_interval']