import sys
import os
import cv2
import random
import shutil

from pathlib import Path
from ultralytics.data.utils import compress_one_image
from PyQt6.QtWidgets import (
    QApplication,
    QMainWindow,
    QLabel,
    QPushButton,
    QFileDialog,
    QWidget,
    QGridLayout,
    QLineEdit,
    QTabWidget,
    QCheckBox
)

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Utils")
        self.resize(640, 320)

        self.central_widget = QWidget()
        self.layout = QGridLayout()
        self.central_widget.setLayout(self.layout)
        self.setCentralWidget(self.central_widget)

        self.tab = QTabWidget()

        self.slicer_wid = SlicerWidget()
        self.optimizer_wid = OptimizerWidget()
        self.sinthetic_wid = SintheticWidget()
        # self.polugon_wid = PolygonWidget()

        self.tab.addTab(self.slicer_wid, "Slicer")
        self.tab.addTab(self.optimizer_wid, "Optimizer")
        self.tab.addTab(self.sinthetic_wid, "Sinthetic")
        # self.tab.addTab(self.polugon_wid, "Polygon")

        self.layout.addWidget(self.tab)
        self.setLayout(self.layout)

# Widget for the video slicer
class SlicerWidget(QWidget):
    def __init__(self):
        super().__init__()

        layout = QGridLayout()
        layout.setSpacing(15)
        
        # Selecting a video folder
        self.vid_l = QLabel("Select the folder with the video files:")
        self.vid_e = QLineEdit(os.getcwd())
        self.vid_b = QPushButton("View")
        self.vid_b.clicked.connect(lambda: self.choose_folder("vid"))
    
        # Selecting output folder
        self.out_l = QLabel("Select the folder to save the frames to:")
        self.out_e = QLineEdit(os.getcwd())
        self.out_b = QPushButton("View")
        self.out_b.clicked.connect(lambda: self.choose_folder("out"))
        
        # Prefix
        self.pre_l = QLabel("Frame prefix:")
        self.pre_e = QLineEdit("img_")

        # Index
        self.ind_l = QLabel("Initial index:")
        self.ind_e = QLineEdit("10000")

        # Frame step
        self.ste_l = QLabel("The frame extraction step:")
        self.ste_e = QLineEdit("120")

        # Start button
        self.start_button = QPushButton("Start")
        self.start_button.clicked.connect(self.start_extraction)

        # Adding to layout
        layout.addWidget(self.vid_l, 0, 0, 1, 8)
        layout.addWidget(self.vid_e, 1, 0, 1, 7)
        layout.addWidget(self.vid_b, 1, 7, 1, 1)

        layout.addWidget(self.out_l, 2, 0, 1, 8)
        layout.addWidget(self.out_e, 3, 0, 1, 7)
        layout.addWidget(self.out_b, 3, 7, 1, 1)

        layout.addWidget(self.pre_l, 4, 0, 1, 1)
        layout.addWidget(self.pre_e, 4, 1, 1, 1)

        layout.addWidget(self.ind_l, 5, 0, 1, 1)
        layout.addWidget(self.ind_e, 5, 1, 1, 1)

        layout.addWidget(self.ste_l, 6, 0, 1, 1)
        layout.addWidget(self.ste_e, 6, 1, 1, 1)

        layout.addWidget(self.start_button, 6, 7, 1, 1)

        self.setLayout(layout)
    
    # Selecting a folder 
    def choose_folder(self, check):
        dir_dialog = QFileDialog()
        dir_dialog.setFileMode(QFileDialog.FileMode.Directory)
        if dir_dialog.exec():
            dir_path = dir_dialog.selectedFiles()[0]
            if check == "vid":
                self.vid_e.setText(dir_path)
            elif check == "out":
                self.out_e.setText(dir_path)
    
    # Initializing variables and starting slicing
    def start_extraction(self):
        video_folder = self.vid_e.text()
        output_folder = self.out_e.text()
        prefix = self.pre_e.text()
        start_index = int(self.ind_e.text())
        frame_step = int(self.ste_e.text())

        video_files = [f for f in os.listdir(video_folder) if f.endswith((".mp4", ".avi"))]
        
        index = start_index

        for video_file in video_files:
            video_path = os.path.join(video_folder, video_file)
            video_output_folder = os.path.join(output_folder, os.path.splitext(video_file)[0])
            
            self.extract_frames(video_path, video_output_folder, prefix, index, frame_step)
            
            index += len(os.listdir(video_output_folder))
    
    # Slicing
    def extract_frames(self, video_path, output_folder, prefix, start_index, frame_step):
        if not os.path.exists(output_folder):
            os.makedirs(output_folder)

        video_capture = cv2.VideoCapture(video_path)
        success, image = video_capture.read()

        count = 0
        frame_number = start_index

        while success:
            if count % frame_step == 0:
                frame_path = os.path.join(output_folder, f"{prefix}{frame_number:05d}.png")
                cv2.imwrite(frame_path, image)
                frame_number += 1
            success, image = video_capture.read()
            count += 1
        
        video_capture.release()

# Widget for the images optimization
class OptimizerWidget(QWidget):
    def __init__(self):
        super().__init__()
        
        layout = QGridLayout()
        # layout.setSpacing(25)

        # Selectting folder
        self.vid_l = QLabel("Select the folder with photos to optimize:")
        self.vid_e = QLineEdit(os.getcwd())
        self.vid_b = QPushButton("View")
        self.vid_b.clicked.connect(self.choose_folder)

        # Start optimize
        self.start_button = QPushButton("Start")
        self.start_button.clicked.connect(self.optimize_images)

        # Adding to layout
        layout.addWidget(self.vid_l, 0, 0, 1, 8)
        layout.addWidget(self.vid_e, 1, 0, 1, 7)
        layout.addWidget(self.vid_b, 1, 7, 1, 1)

        layout.addWidget(QLabel(""), 2, 0, 5, 8) # For free spase

        layout.addWidget(self.start_button, 7, 7, 1, 1)

        self.setLayout(layout)

    def choose_folder(self):
        dir_dialog = QFileDialog()
        dir_dialog.setFileMode(QFileDialog.FileMode.Directory)
        if dir_dialog.exec():
            dir_path = dir_dialog.selectedFiles()[0]
            self.vid_e.setText(dir_path)
    
    def optimize_images(self):
        folder_path = self.vid_e.text()
        if folder_path:
            for f in Path(folder_path).rglob("*.png"):
                compress_one_image(f)

# Widget for creating synthetic images
class SintheticWidget(QWidget):
    def __init__(self):
        super().__init__()

        layout = QGridLayout()
        layout.setSpacing(15)

        # Selectting folder
        self.inp_l = QLabel("Select the folder with the original images and annotation files:")
        self.inp_e = QLineEdit(os.getcwd())
        self.inp_b = QPushButton("View")
        self.inp_b.clicked.connect(lambda: self.choose_folder("inp"))

        self.out_l = QLabel("Select the folder to save synthetic images to:")
        self.out_e = QLineEdit(os.getcwd())
        self.out_b = QPushButton("View")
        self.out_b.clicked.connect(lambda: self.choose_folder("out"))

        self.mas_l = QLabel("Select the folder with masks:")
        self.mas_e = QLineEdit(os.getcwd())
        self.mas_b = QPushButton("View")
        self.mas_b.clicked.connect(lambda: self.choose_folder("mas"))

        # Set checkbox
        self.check = QCheckBox()
        self.check.setChecked(False)
        self.check_l = QLabel("Skip images?")
        self.check.clicked.connect(self.change_state)

        self.pass_l = QLabel("Enter the image selection step:")
        self.pass_e = QLineEdit()
        self.pass_l.setEnabled(False)
        self.pass_e.setEnabled(False)

        # Start button
        self.start_button = QPushButton("Start")
        self.start_button.clicked.connect(self.generate_synthetics)

        layout.addWidget(self.inp_l, 0, 0, 1, 8)
        layout.addWidget(self.inp_e, 1, 0, 1, 7)
        layout.addWidget(self.inp_b, 1, 7, 1, 1)

        layout.addWidget(self.out_l, 2, 0, 1, 8)
        layout.addWidget(self.out_e, 3, 0, 1, 7)
        layout.addWidget(self.out_b, 3, 7, 1, 1)

        layout.addWidget(self.mas_l, 4, 0, 1, 8)
        layout.addWidget(self.mas_e, 5, 0, 1, 7)
        layout.addWidget(self.mas_b, 5, 7, 1, 1)

        layout.addWidget(self.check_l, 6, 0, 1, 4)
        layout.addWidget(self.check, 7, 2, 1, 1)
        
        layout.addWidget(self.pass_l, 6, 5, 1, 3)
        layout.addWidget(self.pass_e, 7, 6, 1, 1)

        layout.addWidget(self.start_button, 7, 7, 1, 1)

        self.setLayout(layout)

    # Synthetic image generation function
    def generate_synthetics(self):
        input_dir = self.inp_e.text()
        output_dir = self.out_e.text()
        masks_dir = self.mas_e.text()

        if input_dir and output_dir and masks_dir:  # Checking whether the fields are full
            masks = [os.path.join(masks_dir, f) for f in os.listdir(masks_dir)]

            if not os.path.exists(output_dir):
                os.makedirs(output_dir)

            # Checking the checkbox
            if self.check.isChecked():
                try:
                    step = int(self.pass_e.text())
                except TypeError:
                    step = 1
            else:
                step = 1

            # The main cycle of reading, combining and copying images.
            # Copying annotation files.
            for i, image_name in enumerate(os.listdir(input_dir)):
                if i % step == 0:
                    if not image_name.endswith((".png", ".jpg", ".jpeg")):
                        continue

                    annotation_name = os.path.splitext(image_name)[0] + ".txt"

                    image_path = os.path.join(input_dir, image_name)
                    annotation_path = os.path.join(input_dir, annotation_name)

                    if os.path.exists(image_path) and os.path.exists(annotation_path):
                        image = cv2.imread(image_path)
                        
                        # Choosing a random mask and resize
                        mask_path = random.choice(masks)
                        mask = cv2.imread(mask_path)
                        mask = cv2.resize(mask, (image.shape[1], image.shape[0]))
                        
                        # Type of merger
                        result = cv2.bitwise_and(image, mask)

                        # Saving the results and copying the annotation
                        output_image_name = f"sin_{image_name}"
                        output_image_path = os.path.join(output_dir, output_image_name)
                        cv2.imwrite(output_image_path, result)

                        output_annotation_name = f"sin_{annotation_name}"
                        output_annotation_path = os.path.join(output_dir, output_annotation_name)
                        shutil.copy(annotation_path, output_annotation_path)

    # Folder selection function
    def choose_folder(self, check):
        dir_dialog = QFileDialog()
        dir_dialog.setFileMode(QFileDialog.FileMode.Directory)
        if dir_dialog.exec():
            dir_path = dir_dialog.selectedFiles()[0]
            if check == "inp":
                self.inp_e.setText(dir_path)
            elif check == "out":
                self.out_e.setText(dir_path)
            elif check == "mas":
                self.mas_e.setText(dir_path)
    
    # The function of changing the status of the checkbox
    def change_state(self):
        if self.check.isChecked():
            self.pass_e.setEnabled(True)
            self.pass_l.setEnabled(True)
        else:
            self.pass_e.setEnabled(False)
            self.pass_l.setEnabled(False)

polygon_points = []
# Widget for creating polygons in an image
class PolygonWidget(QWidget):
    def __init__(self):
        super().__init__()

        layout = QGridLayout()

        # Image selection and launch
        self.img_l = QLabel("Select image:")
        self.img_e = QLineEdit()
        self.img_b = QPushButton("View")
        self.img_b.clicked.connect(self.choose_file)
        self.img_b_start = QPushButton("Open image")
        self.img_b_start.clicked.connect(self.polygon_creator)

        self.pol_l = QLabel("Coordinates of the polygon:")
        self.pol_e = QLineEdit()
        self.pol_b = QPushButton("Copy")
        self.pol_b.clicked.connect(self.copy_to_clipboard)

        self.info_l = QLabel("""Left click - put a point
Right click - save polygon
Scroll click - clear
q - close the image""")

        layout.addWidget(self.img_l, 0, 0)
        layout.addWidget(self.img_e, 1, 0)
        layout.addWidget(self.img_b, 2, 0)
        layout.addWidget(self.img_b_start, 3, 0)
        
        layout.addWidget(self.info_l, 0, 1)
        layout.addWidget(self.pol_l, 1, 1)
        layout.addWidget(self.pol_e, 2, 1)
        layout.addWidget(self.pol_b, 3, 1)

        self.setLayout(layout)

    # Folder selection function
    def choose_file(self):
        image_path, _ = QFileDialog.getOpenFileName(self)
        self.img_e.setText(image_path)
    
    # Copy to clipboard function
    def copy_to_clipboard(self):
        text = self.pol_e.text()
        clipboard = QApplication.clipboard()
        clipboard.setText(text)
    
    # Folder selection function
    def polygon_creator(self):
        image = self.img_e.text()

        if image:
            img_original = cv2.imread(image)
            img_original = cv2.resize(img_original, (640, 640))
            img = img_original.copy()

            def click_event(event, x, y, flags, params):
                global polygon_points
                if event == cv2.EVENT_LBUTTONDOWN:
                    polygon_points.append((x, y))
                    cv2.circle(img, (x, y), 3, (0, 255, 0), -1)
                elif event == cv2.EVENT_RBUTTONDOWN:
                    self.pol_e.setText(f"{polygon_points}")
                elif event == cv2.EVENT_MBUTTONDOWN:
                    polygon_points = []
                    img[:] = img_original[:]
                
            cv2.namedWindow("Polygon Creator")
            cv2.setMouseCallback("Polygon Creator", click_event)

            while True:
                cv2.imshow("Polygon Creator", img)
                
                if cv2.waitKey(1) & 0xFF == ord("q"):
                    break
            
            cv2.destroyAllWindows()

class DetectWidget(QWidget):
    def __init__(self):
        super().__init__()

    layout = QGridLayout()


if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
