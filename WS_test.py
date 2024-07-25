import websocket
import json

WS = 'ws://194.87.111.128:1031/'

con = websocket.create_connection(WS)

message = con.recv()

data = json.loads(message)

print(data)

con.close()