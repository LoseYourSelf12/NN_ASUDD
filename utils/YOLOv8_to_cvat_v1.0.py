from ultralytics import YOLO

import cvat_sdk.auto_annotation as cvataa


_model = YOLO("s_860i_m50-0,67(0,75).pt") # Меняем модель на нашу

spec = cvataa.DetectionFunctionSpec(
    labels=[cvataa.label_spec(name, id) for id, name in _model.names.items()],
)

def _yolo_to_cvat(results):
    for result in results:
        for box, label in zip(result.boxes.xyxy, result.boxes.cls):
            yield cvataa.rectangle(int(label.item()), [p.item() for p in box])

def detect(context, image):
    return list(_yolo_to_cvat(_model.predict(source=image, verbose=False, device='cpu', conf=0.5, imgsz=640)))

# $ENV:PASS = Read-Host -MaskInput 
# cvat-cli --server-host localhost:8080 --auth admin auto-annotate 254 --function-file .\YOLOv8_to_cvat.py --allow-unmatched-labels  --clear-existing
