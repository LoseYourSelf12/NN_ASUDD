import requests
import keyboard  # Для захвата нажатий клавиш
import time

SERVER_URL = 'http://127.0.0.1:5000/detect/'

def send_request(camera_id, action):
    """Функция для отправки POST запроса на сервер."""
    url = f"{SERVER_URL}{camera_id}"
    data = {'start_detection': True} if action == 'start' else {'stop_detection': True}
    try:
        response = requests.post(url, json=data)
        print(f"Response from {camera_id}: {response.json()}")
    except Exception as e:
        print(f"Failed to send request: {e}")

def simulate_events():
    """Функция симулятора, реагирующая на нажатия клавиш."""
    print("Симулятор запущен. Управление: q (старт детекции камеры 1), Shift+q (стоп детекции камеры 1), и т.д.")
    
    while True:
        # Камера 1
        if keyboard.is_pressed('q'):
            send_request('camera_1', 'start')
        if keyboard.is_pressed('shift+q'):
            send_request('camera_1', 'stop')

        # Камера 2
        if keyboard.is_pressed('w'):
            send_request('camera_2', 'start')
        if keyboard.is_pressed('shift+w'):
            send_request('camera_2', 'stop')

        # Камера 3
        if keyboard.is_pressed('e'):
            send_request('camera_3', 'start')
        if keyboard.is_pressed('shift+e'):
            send_request('camera_3', 'stop')

        # Камера 4
        if keyboard.is_pressed('r'):
            send_request('camera_4', 'start')
        if keyboard.is_pressed('shift+r'):
            send_request('camera_4', 'stop')

        # Добавляем небольшую задержку, чтобы не было лишних запросов
        time.sleep(0.1)

if __name__ == "__main__":
    simulate_events()
