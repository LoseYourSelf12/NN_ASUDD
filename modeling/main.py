import requests
import json
import time

# Адрес сервера статистики (RestStateAddr)
server_url = "http://localhost:1038/setComm"  # Заменить на реальный URL

# Функция для отправки данных детектора
def send_traffic_data(straight, left, right, det_no=1):
    # Формирование данных в формате JSON
    data = {
        "DetNo": det_no,
        "CommandStr": f"tcpa: TRAFFIC_DATA Straight: {straight}%, Left: {left}%, Right: {right}%",
    }
    
    try:
        # Отправляем POST-запрос на сервер
        response = requests.post(server_url, json=data)
        
        # Проверяем ответ от сервера
        if response.status_code == 200:
            print("Данные успешно отправлены:", data)
        else:
            print(f"Ошибка при отправке данных. Статус код: {response.status_code}")
    
    except Exception as e:
        print(f"Ошибка: {e}")

# Симуляция передачи данных
if __name__ == "__main__":
    while True:
        # Ввод данных вручную
        straight = 0.7
        left = 0.2
        right = 0.1

        # Отправка данных
        send_traffic_data(straight, left, right)

        # Интервал перед следующей отправкой
        time.sleep(5)
