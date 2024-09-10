import requests

# Адрес сервера статистики
server_url = "http://www.sistema-complex.ru/"  # Заменить на реальный URL

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