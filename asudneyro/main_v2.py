import threading
import time
import signal
import sys
from flask import Flask, request

# Импортируем VideoProcessor из вашего модуля
from modules.VideoProcessor import VideoProcessor
from modules.config import LoadConfig


config = LoadConfig()

app = Flask(__name__)

# Словарь для хранения обработчиков видеопотоков по ID камеры
video_processors = {
    'camera_1': VideoProcessor(video_source=config.config['test_vid'], camera_id='camera_1'),
    'camera_2': VideoProcessor(video_source=config.config['test_vid'], camera_id='camera_2'),
    'camera_3': VideoProcessor(video_source=config.config['test_vid'], camera_id='camera_3'),
    'camera_4': VideoProcessor(video_source=config.config['test_vid'], camera_id='camera_4')
}



@app.route('/detect/<camera_id>', methods=['POST'])
def detect_event(camera_id):
    """Обработка запросов для управления детекцией на видеопотоках."""
    if camera_id not in video_processors:
        return {"message": f"Invalid camera_id: {camera_id}"}, 400

    data = request.json
    if 'start_detection' in data:
        video_processors[camera_id].detection_event.set()  # Включаем детекцию для камеры
        return {"message": f"Detection started for {camera_id}"}, 200
    elif 'stop_detection' in data:
        video_processors[camera_id].detection_event.clear()  # Останавливаем детекцию для камеры
        return {"message": f"Detection stopped for {camera_id}"}, 200
    else:
        return {"message": "Invalid command"}, 400

def run_server():
    """Запуск Flask сервера в отдельном потоке."""
    app.run(host='0.0.0.0', port=5000)

def signal_handler(sig, frame):
    """Обработчик сигнала для корректного завершения программы."""
    print("Завершаю работу...")
    for vp in video_processors.values():
        vp.stop()  # Остановка всех видеопотоков
    
    # После остановки всех потоков просто завершаем работу программы
    sys.exit(0)


if __name__ == "__main__":
    signal.signal(signal.SIGINT, signal_handler)

    try:
        # Запуск потоков для всех камер
        for vp in video_processors.values():
            vp.start()

        # Запуск сервера в отдельном потоке
        server_thread = threading.Thread(target=run_server, daemon=True)
        server_thread.start()

        while True:
            time.sleep(1)

    except KeyboardInterrupt:
        print("Завершение работы...")
        server_thread.join()     
        