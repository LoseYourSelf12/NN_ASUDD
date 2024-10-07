import cv2

import numpy as np

from modules.config import LoadConfig


# Загрузка конфиг файла
config = LoadConfig()

# Создание внешней маски из полигона
mask = np.zeros((config.config['imgsz'], config.config['imgsz']), dtype=np.uint8)
cv2.fillPoly(mask, [np.array(config.config['out_region'])], 255)
background = cv2.bitwise_not(mask)
background = cv2.cvtColor(background, cv2.COLOR_GRAY2BGR)


def img_processing(img_in, show_event_state):
    """
    Обработка изображений:
    - изменение размера
    - наложение маски
    - отрисовка полигонов и линий
    """
    SHOW_EVENT_STATE = show_event_state
    # Изменение размера
    img_out = cv2.resize(img_in, (config.config['imgsz'], config.config['imgsz']))

    # Наложение маски
    img_out = cv2.bitwise_and(img_out, img_out, mask=mask)

    # Измененеие фона
    img_out = cv2.add(img_out, background)

    # Отрисовка полигонов, если необходимо
    if SHOW_EVENT_STATE:
        # Внешний полигон
        cv2.polylines(img_out, [np.array(config.config['out_region'])], isClosed=True, color=(0, 255, 0), thickness=2)
        
        # Внутренние полигоны
        for in_reg in config.config['in_regions']:
            cv2.polylines(img_out, [np.array(in_reg)], isClosed=True, color=(255, 0, 0), thickness=2)

        # Линии пересечения
        for in_line in config.config['lines_with_directions']:
            cv2.polylines(img_out, [np.array(in_line['line'])], isClosed=True, color=(0, 0, 255), thickness=2)

        # Отображение картинки
        cv2.imshow('Debug Images', img_out)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            SHOW_EVENT_STATE = False
            cv2.destroyAllWindows()
    
    return img_out, SHOW_EVENT_STATE