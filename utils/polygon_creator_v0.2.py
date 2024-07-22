import sys
import json
import cv2
from PyQt6.QtWidgets import QApplication, QMainWindow, QPushButton, QFileDialog, QLabel, QCheckBox, QLineEdit, QVBoxLayout, QWidget, QHBoxLayout, QMessageBox, QGridLayout
from PyQt6.QtGui import QPixmap, QImage, QMouseEvent, QPainter, QPen
from PyQt6.QtCore import Qt, QPoint

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()

        self.setWindowTitle("PolyCreator v1.0")
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

        self.config_name_input = QLineEdit("config", self)
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
        self.image_label.mouseMoveEvent = self.image_mouseMoveEvent

        layout.addWidget(self.image_label)

        # Sidebar for additional controls
        sidebar_layout = QVBoxLayout()

        self.checkbox_outer_polygon = QCheckBox("Creating an external polygon", self)
        self.checkbox_inner_polygon = QCheckBox("Creating internal polygons", self)
        self.checkbox_outer_polygon.setChecked(True)
        self.checkbox_outer_polygon.stateChanged.connect(self.toggle_polygon_checkboxes)
        self.checkbox_inner_polygon.stateChanged.connect(self.toggle_polygon_checkboxes)

        sidebar_layout.addWidget(self.checkbox_outer_polygon)
        sidebar_layout.addWidget(self.checkbox_inner_polygon)

        self.outer_polygon_label = QLabel("External polygon: []", self)
        self.inner_polygon_label = QLabel("Internal polygons: []", self)
        self.outer_polygon_label.setFixedHeight(40)
        self.inner_polygon_label.setFixedHeight(40)
        self.outer_polygon_label.setWordWrap(True)
        self.inner_polygon_label.setWordWrap(True)

        sidebar_layout.addWidget(self.outer_polygon_label)
        sidebar_layout.addWidget(self.inner_polygon_label)

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

        # Initialize polygon creation variables
        self.drawing_polygon = False
        self.current_polygon = []
        self.polygons = {"outer": None, "inner": []}
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

        with open(config_name + ".json", 'w') as f:
            json.dump(config, f, indent=4)

        QMessageBox.information(self, "Saving...", "Polygons are saved to a configuration file!")

    def image_mousePressEvent(self, event: QMouseEvent):
        if event.button() == Qt.MouseButton.LeftButton:
            pos = event.position().toPoint()
            self.current_polygon.append([pos.x(), pos.y()])
            self.update_image()
        elif event.button() == Qt.MouseButton.MiddleButton:
            if self.current_polygon:
                self.current_polygon.pop()
                self.update_image()
        elif event.button() == Qt.MouseButton.RightButton:
            if self.current_polygon:
                if self.checkbox_outer_polygon.isChecked():
                    self.polygons["outer"] = self.current_polygon
                    self.checkbox_outer_polygon.setChecked(False)
                elif self.checkbox_inner_polygon.isChecked():
                    self.polygons["inner"].append(self.current_polygon)
                self.current_polygon = []
                self.update_image()
            self.update_labels()

    def image_mouseMoveEvent(self, event: QMouseEvent):
        pass  # Can be implemented if needed for real-time drawing feedback

    def update_image(self):
        # Clear the image
        img = self.original_img.copy()
        painter = QPainter()
        q_img = QImage(img.data, img.shape[1], img.shape[0], QImage.Format.Format_RGB888).rgbSwapped()
        painter.begin(q_img)
        pen_outer = QPen(Qt.GlobalColor.red)
        pen_outer.setWidth(2)
        pen_inner = QPen(Qt.GlobalColor.green)
        pen_inner.setWidth(2)

        # Draw all the polygons
        if self.polygons["outer"]:
            painter.setPen(pen_outer)
            for i in range(len(self.polygons["outer"])):
                p1 = QPoint(self.polygons["outer"][i][0], self.polygons["outer"][i][1])
                p2 = QPoint(self.polygons["outer"][(i + 1) % len(self.polygons["outer"])][0], self.polygons["outer"][(i + 1) % len(self.polygons["outer"])][1])
                painter.drawLine(p1, p2)
            # Add polygon number label
            first_point = self.polygons["outer"][0]
            painter.drawText(first_point[0], first_point[1], "1")

        for index, polygon in enumerate(self.polygons["inner"]):
            painter.setPen(pen_inner)
            for i in range(len(polygon)):
                p1 = QPoint(polygon[i][0], polygon[i][1])
                p2 = QPoint(polygon[(i + 1) % len(polygon)][0], polygon[(i + 1) % len(polygon)][1])
                painter.drawLine(p1, p2)
            # Add polygon number label
            first_point = polygon[0]
            painter.drawText(first_point[0], first_point[1], str(index + 2))

        # Draw the current polygon being created
        if self.current_polygon:
            if self.checkbox_outer_polygon.isChecked():
                painter.setPen(pen_outer)
            elif self.checkbox_inner_polygon.isChecked():
                painter.setPen(pen_inner)
            for i in range(len(self.current_polygon)):
                p1 = QPoint(self.current_polygon[i][0], self.current_polygon[i][1])
                p2 = QPoint(self.current_polygon[(i + 1) % len(self.current_polygon)][0], self.current_polygon[(i + 1) % len(self.current_polygon)][1])
                painter.drawLine(p1, p2)

        painter.end()
        self.image_label.setPixmap(QPixmap.fromImage(q_img))
        self.update_labels()

    def update_labels(self):
        self.outer_polygon_label.setText(f"External polygon: {self.polygons['outer'] if self.polygons['outer'] else '[]'}")
        self.inner_polygon_label.setText(f"Internal polygons: {self.polygons['inner'] if self.polygons['inner'] else '[]'}")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
