import sys
import json
import cv2
from PyQt6.QtWidgets import QApplication, QMainWindow, QPushButton, QFileDialog, QLabel, QCheckBox, QLineEdit, QVBoxLayout, QWidget, QHBoxLayout, QMessageBox, QGridLayout
from PyQt6.QtGui import QPixmap, QImage, QMouseEvent, QPainter, QPen
from PyQt6.QtCore import Qt, QPoint
import numpy as np

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("PolyCreator v0.3")
        self.setGeometry(100, 100, 300, 150)
        self.initUI()

    def initUI(self):
        layout = QGridLayout()

        self.checkbox_stream = QCheckBox("... from stream", self)
        self.checkbox_stream.stateChanged.connect(self.toggle_file_selection)
        layout.addWidget(self.checkbox_stream, 0, 5)

        self.lable_1 = QLabel("Image selection...")
        layout.addWidget(self.lable_1, 0, 0, 1, 5)

        self.file_input = QLineEdit(self)
        layout.addWidget(self.file_input, 1, 0, 1, 5)

        self.browse_button = QPushButton("View", self)
        self.browse_button.clicked.connect(self.browse_files)
        layout.addWidget(self.browse_button, 1, 5, 1, 1)

        self.lable_2 = QLabel("Config file .json")
        layout.addWidget(self.lable_2, 2, 0, 1, 6)

        self.config_name_input = QLineEdit("configure/config", self)
        layout.addWidget(self.config_name_input, 3, 0, 1, 6)

        self.start_button = QPushButton("Create", self)
        self.start_button.clicked.connect(self.start_processing)
        layout.addWidget(self.start_button, 4, 4, 1, 1)

        self.close_button = QPushButton("Close", self)
        self.close_button.clicked.connect(self.close)
        layout.addWidget(self.close_button, 4, 5, 1, 1)

        container = QWidget()
        container.setLayout(layout)
        self.setCentralWidget(container)

    def toggle_file_selection(self):
        if self.checkbox_stream.isChecked():
            self.file_input.setDisabled(True)
            self.browse_button.setDisabled(True)
        else:
            self.file_input.setDisabled(False)
            self.browse_button.setDisabled(False)

    def browse_files(self):
        file_dialog = QFileDialog(self)
        file_dialog.setNameFilter("Images (*.jpeg *.jpg *.png)")
        if file_dialog.exec():
            file_path = file_dialog.selectedFiles()[0]
            self.file_input.setText(file_path)

    def start_processing(self):
        config_name = self.config_name_input.text()
        config = {}
        if self.checkbox_stream.isChecked():
            with open(config_name + ".json", 'r') as f:
                config = json.load(f)
            stream_url = config.get("cam_ip", "")
            cap = cv2.VideoCapture(stream_url)
            ret, frame = cap.read()
            if ret:
                self.display_image(frame)
            cap.release()
        else:
            file_path = self.file_input.text()
            image = cv2.imread(file_path)
            self.display_image(image)

    def display_image(self, img):
        # Resize the image to the specified size in the config
        config_name = self.config_name_input.text()
        with open(config_name + ".json", 'r') as f:
            config = json.load(f)
        img_size = config.get("imgsz", 640)
        img = cv2.resize(img, (img_size, img_size))

        # Convert image to QImage and display it
        self.original_img = img.copy()  # Save the original image for redrawing
        height, width, channel = img.shape
        bytes_per_line = 3 * width
        q_img = QImage(img.data, width, height, bytes_per_line, QImage.Format.Format_RGB888).rgbSwapped()
        pixmap = QPixmap.fromImage(q_img)

        self.new_window = QMainWindow()
        self.new_window.setWindowTitle("Processed Image")
        self.new_window.setGeometry(100, 100, img_size + 200, img_size)

        layout = QHBoxLayout()
        self.image_label = QLabel(self)
        self.image_label.setPixmap(pixmap)
        self.image_label.mousePressEvent = self.image_mousePressEvent

        layout.addWidget(self.image_label)

        # Sidebar for additional controls
        sidebar_layout = QVBoxLayout()

        self.checkbox_outer_polygon = QCheckBox("Creating an external polygon", self)
        self.checkbox_inner_polygon = QCheckBox("Creating internal polygons", self)
        self.checkbox_line_direction = QCheckBox("Draw line with direction", self)
        self.checkbox_outer_polygon.setChecked(True)
        self.checkbox_outer_polygon.stateChanged.connect(self.toggle_polygon_checkboxes)
        self.checkbox_inner_polygon.stateChanged.connect(self.toggle_polygon_checkboxes)

        sidebar_layout.addWidget(self.checkbox_outer_polygon)
        sidebar_layout.addWidget(self.checkbox_inner_polygon)
        sidebar_layout.addWidget(self.checkbox_line_direction)

        self.outer_polygon_label = QLabel("External polygon: []", self)
        self.inner_polygon_label = QLabel("Internal polygons: []", self)
        self.line_label = QLabel("Lines with directions: []", self)
        self.outer_polygon_label.setFixedHeight(40)
        self.inner_polygon_label.setFixedHeight(40)
        self.line_label.setFixedHeight(40)
        self.outer_polygon_label.setWordWrap(True)
        self.inner_polygon_label.setWordWrap(True)
        self.line_label.setWordWrap(True)

        sidebar_layout.addWidget(self.outer_polygon_label)
        sidebar_layout.addWidget(self.inner_polygon_label)
        sidebar_layout.addWidget(self.line_label)

        save_button = QPushButton("Save", self)
        save_button.clicked.connect(self.save_polygons)
        close_button = QPushButton("Close", self)
        close_button.clicked.connect(self.new_window.close)

        sidebar_layout.addWidget(save_button)
        sidebar_layout.addWidget(close_button)

        layout.addLayout(sidebar_layout)

        container = QWidget()
        container.setLayout(layout)
        self.new_window.setCentralWidget(container)
        self.new_window.show()

        # Initialize polygon and line creation variables
        self.drawing_polygon = False
        self.current_polygon = []
        self.current_line = []
        self.polygons = {"outer": None, "inner": [], "lines_with_directions": []}
        self.update_labels()

    def toggle_polygon_checkboxes(self):
        if self.checkbox_outer_polygon.isChecked():
            self.checkbox_inner_polygon.setChecked(False)
        elif self.checkbox_inner_polygon.isChecked():
            self.checkbox_outer_polygon.setChecked(False)

    def save_polygons(self):
        config_name = self.config_name_input.text()
        with open(config_name + ".json", 'r') as f:
            config = json.load(f)

        if self.polygons["outer"]:
            config["out_region"] = self.polygons["outer"]
        if self.polygons["inner"]:
            config["in_regions"] = self.polygons["inner"]
        if self.polygons["lines_with_directions"]:
            config["lines_with_directions"] = self.polygons["lines_with_directions"]

        with open(config_name + ".json", 'w') as f:
            json.dump(config, f, indent=4)

        QMessageBox.information(self, "Saving...", "Polygons and lines are saved to a configuration file!")

    def image_mousePressEvent(self, event: QMouseEvent):
        pos = event.position().toPoint()
        
        if event.button() == Qt.MouseButton.LeftButton:
            # Рисование полигонов
            if self.checkbox_outer_polygon.isChecked() or self.checkbox_inner_polygon.isChecked():
                self.drawing_polygon = True
                self.current_polygon.append([pos.x(), pos.y()])
                self.update_image()  # Обновление изображения после каждого клика
            
            # Рисование линии
            elif self.checkbox_line_direction.isChecked():
                if len(self.current_line) < 2:
                    self.current_line.append([pos.x(), pos.y()])
                else:
                    p1, p2 = self.current_line
                    third_point = [pos.x(), pos.y()]
                    mid_point = [(p1[0] + p2[0]) // 2, (p1[1] + p2[1]) // 2]

                    # Рассчитываем вектор направления перпендикуляра
                    line_vec = np.array([p2[0] - p1[0], p2[1] - p1[1]])
                    perp_vec = np.array([-line_vec[1], line_vec[0]])
                    perp_vec = perp_vec / np.linalg.norm(perp_vec)

                    # Рассчитываем точку для стрелки
                    direction_point = mid_point + perp_vec * 50

                    # Добавляем отрезок с направлением
                    self.current_line.append([pos.x(), pos.y()])
                    self.polygons["lines_with_directions"].append({
                        "line": [p1, p2],
                        "third_point": third_point, 
                        "direction": [int(direction_point[0]), int(direction_point[1])]
                    })
                    self.current_line = []
                self.update_image()

        elif event.button() == Qt.MouseButton.RightButton:
            # Завершение рисования полигона
            if self.drawing_polygon:
                if self.checkbox_outer_polygon.isChecked():
                    self.polygons["outer"] = self.current_polygon
                elif self.checkbox_inner_polygon.isChecked():
                    self.polygons["inner"].append(self.current_polygon)
                self.current_polygon = []
                self.drawing_polygon = False
                self.update_labels()
                self.update_image()
            
            # Завершение рисования линии и добавление её в config
            elif self.checkbox_line_direction.isChecked():
                if len(self.current_line) == 2:
                    p1, p2 = self.current_line
                    mid_point = [(p1[0] + p2[0]) // 2, (p1[1] + p2[1]) // 2]
                    third_point = [pos.x(), pos.y()]

                    # Рассчитываем направление
                    line_vec = np.array([p2[0] - p1[0], p2[1] - p1[1]])
                    perp_vec = np.array([-line_vec[1], line_vec[0]])
                    perp_vec = perp_vec / np.linalg.norm(perp_vec)

                    direction_point = mid_point + perp_vec * 50

                    # Добавляем линию и направление в config
                    self.polygons["lines_with_directions"].append({
                        "line": [p1, p2],
                        "third_point": third_point,
                        "direction": [int(direction_point[0]), int(direction_point[1])]
                    })
                    self.current_line = []
                    self.update_image()


    def update_labels(self):
        self.outer_polygon_label.setText(f"External polygon: {self.polygons['outer']}")
        self.inner_polygon_label.setText(f"Internal polygons: {self.polygons['inner']}")
        self.line_label.setText(f"Lines with directions: {self.polygons['lines_with_directions']}")

    def update_image(self):
        img_copy = self.original_img.copy()

        # Рисование внешнего полигона
        if self.polygons["outer"]:
            pts = np.array(self.polygons["outer"], np.int32)
            pts = pts.reshape((-1, 1, 2))
            cv2.polylines(img_copy, [pts], isClosed=True, color=(0, 0, 255), thickness=2)  # Red outer polygon

        # Рисование внутренних полигонов
        for polygon in self.polygons["inner"]:
            pts = np.array(polygon, np.int32)
            pts = pts.reshape((-1, 1, 2))
            cv2.polylines(img_copy, [pts], isClosed=True, color=(0, 255, 0), thickness=2)  # Green inner polygons

        # Рисование линий с направлениями
        for line_data in self.polygons["lines_with_directions"]:
            line = line_data["line"]
            direction = line_data["direction"]
            cv2.line(img_copy, tuple(line[0]), tuple(line[1]), (255, 255, 0), 2)  # Бирюзовая линия
            cv2.arrowedLine(img_copy, ((line[0][0] + line[1][0]) // 2, (line[0][1] + line[1][1]) // 2),
                            tuple(direction), (255, 255, 0), 2)  # Стрелка для направления

        # Рисование текущего полигона
        if self.current_polygon:
            pts = np.array(self.current_polygon, np.int32)
            pts = pts.reshape((-1, 1, 2))
            if self.checkbox_outer_polygon.isChecked():
                cv2.polylines(img_copy, [pts], isClosed=False, color=(0, 0, 255), thickness=2)  # Красный (внешний)
            elif self.checkbox_inner_polygon.isChecked():
                cv2.polylines(img_copy, [pts], isClosed=False, color=(0, 255, 0), thickness=2)  # Зеленый (внутренний)

        # Рисование текущей линии
        if len(self.current_line) == 1:
            p1 = self.current_line[0]
            cv2.circle(img_copy, tuple(p1), 5, (255, 255, 0), -1)  # Точка начала линии

        # Обновление QImage
        height, width, channel = img_copy.shape
        bytes_per_line = 3 * width
        q_img = QImage(img_copy.data, width, height, bytes_per_line, QImage.Format.Format_RGB888).rgbSwapped()
        pixmap = QPixmap.fromImage(q_img)
        self.image_label.setPixmap(pixmap)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
