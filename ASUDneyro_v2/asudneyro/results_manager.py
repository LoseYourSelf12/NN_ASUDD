import numpy as np
import logging

class LineCrossingCounter:
    def __init__(self, lines, phase_duration, cycle_duration, camera_id):
        """
        lines: список линий, через которые нужно считать пересечение
        phase_duration: длительность одной фазы в секундах
        cycle_duration: общее время цикла (или число фреймов для одной фазы)
        camera_id: ID камеры, к которой привязан счетчик
        """
        self.lines = [np.array(line['points']) for line in lines]
        self.camera_id = camera_id
        self.phase_duration = phase_duration
        self.cycle_duration = cycle_duration
        self.crossed_count = {line_name: 0 for line_name in [line['name'] for line in lines]}

    def check_crossing(self, tracked_objects):
        """
        Проверяет пересечения линий объектами и увеличивает счетчик пересечений.
        tracked_objects: список трекнутых объектов на кадре
        """
        for obj in tracked_objects:
            for line_name, line_points in zip(self.crossed_count.keys(), self.lines):
                if self._has_crossed_line(obj['bbox'], line_points):
                    self.crossed_count[line_name] += 1
                    logging.info(f"Объект пересек линию {line_name} на камере {self.camera_id}")

    def _has_crossed_line(self, bbox, line_points):
        """Проверяет, пересек ли объект линию. Условие основано на положении bbox и линии."""
        # Логика определения пересечения (например, центр bbox должен пересечь линию)
        bbox_center = np.array([(bbox[0] + bbox[2]) / 2, (bbox[1] + bbox[3]) / 2])
        line_start, line_end = line_points
        # Проверим, пересек ли центр bbox линию (например, через векторное произведение)
        return self._crosses_line(bbox_center, line_start, line_end)

    def _crosses_line(self, point, line_start, line_end):
        """Возвращает True, если точка пересекает линию."""
        # Векторное произведение для определения пересечения
        return np.cross(line_end - line_start, point - line_start) == 0

    def reset_counts(self):
        """Сбрасывает счетчики пересечений для новой фазы или цикла."""
        self.crossed_count = {line_name: 0 for line_name in self.crossed_count.keys()}

    def get_results(self):
        """Возвращает количество пересечений для каждой линии."""
        return self.crossed_count
