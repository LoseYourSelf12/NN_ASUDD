import time

from modules.VideoProcessor import VideoProcessor
from modules.config import LoadConfig

config = LoadConfig()



if __name__ == '__main__':

    # Инициализируем камеры
    camera_1 = VideoProcessor(video_source=config.config['test_vid'])

    try:
        # Запуск камер
        camera_1.start()
        
        # Работаем в основном потоке, пока не потребуется завершить работу
        while True:
            time.sleep(1)

    except KeyboardInterrupt:
        # Остановка при нажатии Ctrl+C
        camera_1.stop()
        print('Работа завершена')
