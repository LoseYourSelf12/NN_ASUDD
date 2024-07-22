import sys
import json
from PyQt6.QtWidgets import (QApplication, QWidget, QVBoxLayout, QHBoxLayout, QLineEdit, QLabel,
                             QPushButton, QCheckBox, QFileDialog, QMessageBox, QScrollArea, QFrame, 
                             QButtonGroup, QInputDialog)
from PyQt6.QtCore import Qt


class JsonEditor(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle('JSON Editor')
        self.setGeometry(100, 100, 800, 600)
        
        self.file_path = None
        self.json_data = {}
        
        self.initUI()

    def initUI(self):
        self.layout = QVBoxLayout()
        
        self.open_button = QPushButton('Open JSON File')
        self.open_button.clicked.connect(self.open_json_file)
        self.layout.addWidget(self.open_button)

        self.new_button = QPushButton('Create New JSON File')
        self.new_button.clicked.connect(self.create_new_json_file)
        self.layout.addWidget(self.new_button)

        self.setLayout(self.layout)

    def open_json_file(self):
        file_dialog = QFileDialog()
        file_path, _ = file_dialog.getOpenFileName(self, 'Open JSON File', 'configure/', 'JSON Files (*.json)')
        if file_path:
            self.file_path = file_path
            self.load_json_data()
            self.open_editor_window()

    def create_new_json_file(self):
        file_dialog = QFileDialog()
        dir_path = file_dialog.getExistingDirectory(self, 'Select Directory')
        if dir_path:
            file_name, ok = QInputDialog.getText(self, 'New JSON File', 'Enter file name:')
            if ok and file_name:
                self.file_path = f'{dir_path}/{file_name}.json'
                self.json_data = {
                    "out_region": [],
                    "in_regions": []
                }
                self.open_editor_window()

    def load_json_data(self):
        with open(self.file_path, 'r') as file:
            self.json_data = json.load(file)
        self.original_out_region = self.json_data.get("out_region", [])
        self.original_in_regions = self.json_data.get("in_regions", [])

    def save_json_data(self):
        try:
            with open(self.file_path, 'w') as file:
                json.dump(self.json_data, file, indent=4)
            QMessageBox.information(self, 'Success', 'JSON file saved successfully!')
        except Exception as e:
            QMessageBox.critical(self, 'Error', f'Failed to save JSON file: {str(e)}')

    def open_editor_window(self):
        self.editor_window = QWidget()
        self.editor_window.setWindowTitle('Edit JSON File')
        self.editor_window.setGeometry(100, 100, 800, 600)

        layout = QVBoxLayout()
        scroll = QScrollArea()
        scroll.setWidgetResizable(True)
        scroll_content = QWidget()
        self.form_layout = QVBoxLayout(scroll_content)

        self.populate_form()

        scroll.setWidget(scroll_content)
        layout.addWidget(scroll)

        buttons_layout = QHBoxLayout()

        add_row_button = QPushButton('Add Row')
        add_row_button.clicked.connect(self.add_row)
        buttons_layout.addWidget(add_row_button)

        delete_rows_button = QPushButton('Delete Rows')
        delete_rows_button.clicked.connect(self.delete_rows)
        buttons_layout.addWidget(delete_rows_button)

        save_button = QPushButton('Save')
        save_button.clicked.connect(self.save_json_file)
        buttons_layout.addWidget(save_button)

        open_button = QPushButton('Open')
        open_button.clicked.connect(self.confirm_open)
        buttons_layout.addWidget(open_button)

        close_button = QPushButton('Close')
        close_button.clicked.connect(self.close_editor)
        buttons_layout.addWidget(close_button)

        layout.addLayout(buttons_layout)
        self.editor_window.setLayout(layout)
        self.editor_window.show()

    def populate_form(self):
        for key, value in self.json_data.items():
            if key in ["out_region", "in_regions"]:
                continue  # Пропускаем эти поля, они не должны редактироваться здесь
            self.add_form_row(key, value)

    def add_form_row(self, key="", value=""):
        row_layout = QHBoxLayout()

        if key:
            key_label = QLabel(key)
            row_layout.addWidget(key_label)
        else:
            key_edit = QLineEdit(key)
            row_layout.addWidget(key_edit)

        value_edit = QLineEdit(str(value))
        row_layout.addWidget(value_edit)

        type_group = QButtonGroup()
        type_str = QCheckBox("str")
        type_int = QCheckBox("int64")
        type_float = QCheckBox("float64")
        type_delete = QCheckBox("Delete")

        type_group.addButton(type_str)
        type_group.addButton(type_int)
        type_group.addButton(type_float)
        type_group.addButton(type_delete)

        type_str.setChecked(True)

        type_str.toggled.connect(lambda: self.handle_type_toggle(type_str, type_int, type_float))
        type_int.toggled.connect(lambda: self.handle_type_toggle(type_int, type_str, type_float))
        type_float.toggled.connect(lambda: self.handle_type_toggle(type_float, type_str, type_int))

        if isinstance(value, str):
            type_str.setChecked(True)
        elif isinstance(value, int):
            type_int.setChecked(True)
        elif isinstance(value, float):
            type_float.setChecked(True)

        row_layout.addWidget(type_str)
        row_layout.addWidget(type_int)
        row_layout.addWidget(type_float)
        row_layout.addWidget(type_delete)

        row_frame = QFrame()
        row_frame.setLayout(row_layout)
        self.form_layout.addWidget(row_frame)

    def handle_type_toggle(self, toggled_button, other_button1, other_button2):
        if toggled_button.isChecked():
            other_button1.setChecked(False)
            other_button2.setChecked(False)

    def add_row(self):
        self.add_form_row()

    def delete_rows(self):
        for i in reversed(range(self.form_layout.count())):
            row_frame = self.form_layout.itemAt(i).widget()
            type_delete = row_frame.findChildren(QCheckBox)[3]
            if type_delete.isChecked():
                self.form_layout.removeWidget(row_frame)
                row_frame.deleteLater()

    def save_json_file(self):
        self.json_data.clear()
        error = False

        for i in range(self.form_layout.count()):
            row_frame = self.form_layout.itemAt(i).widget()
            key_widget = row_frame.findChildren((QLabel, QLineEdit))[0]
            value_edit = row_frame.findChild(QLineEdit)
            type_group = row_frame.findChildren(QCheckBox)

            key = key_widget.text() if isinstance(key_widget, QLabel) else key_widget.text()
            value = value_edit.text()
            value_type = None

            if type_group[0].isChecked():
                value_type = str
            elif type_group[1].isChecked():
                value_type = int
            elif type_group[2].isChecked():
                value_type = float

            try:
                if value_type:
                    self.json_data[key] = value_type(value)
            except ValueError:
                error = True
                QMessageBox.critical(self, 'Error', f'Invalid data type for key: {key}')
                break

        if not error:
            # Восстановление исходных значений для out_region и in_regions
            self.json_data["out_region"] = self.original_out_region
            self.json_data["in_regions"] = self.original_in_regions
            self.save_json_data()

    def confirm_open(self):
        reply = QMessageBox.question(self, 'Open File', 'Unsaved changes will be lost. Continue?', QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No)
        if reply == QMessageBox.StandardButton.Yes:
            self.editor_window.close()
            self.open_json_file()

    def close_editor(self):
        reply = QMessageBox.question(self, 'Close', 'Unsaved changes will be lost. Continue?', QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No)
        if reply == QMessageBox.StandardButton.Yes:
            self.editor_window.close()


if __name__ == '__main__':
    app = QApplication(sys.argv)
    editor = JsonEditor()
    editor.show()
    sys.exit(app.exec())
