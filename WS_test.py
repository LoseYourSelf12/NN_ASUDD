import websocket
import json

def on_message(ws, message):
    data = json.loads(message)
    print(f"Received message: {data}")

def on_error(ws, error):
    print(f"Error: {error}")

def on_close(ws, close_status_code, close_msg):
    print(f"Connection closed: {close_status_code}, {close_msg}")

def on_open(ws):
    print("Connection opened")

if __name__ == "__main__":
    websocket.enableTrace(True)
    ws = websocket.WebSocketApp("ws://localhost:8000/ws",
                                on_open=on_open,
                                on_message=on_message,
                                on_error=on_error,
                                on_close=on_close)
    ws.run_forever()
