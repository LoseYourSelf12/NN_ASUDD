from fastapi import FastAPI, WebSocket, WebSocketDisconnect, HTTPException
from fastapi.responses import JSONResponse
import asyncio
import json
from datetime import datetime

app = FastAPI()

# Глобальные переменные
progNo = 1
taktLen = 4
taktNo = 0

# Маппинг progNo на длительность такта в секундах
progNo_to_taktLen = {
    0: 2,
    1: 4,
    2: 6,
    3: 8
}

# Клиенты для уведомления о смене progNo
clients = []

@app.post("/update_prog_no")
async def update_prog_no(prog_no_update: dict):
    global progNo, taktLen, taktNo
    if "progNo" in prog_no_update:
        progNo = prog_no_update["progNo"]
        taktLen = progNo_to_taktLen[progNo]
        taktNo = 0  # Сброс номера такта
        # Уведомление клиентов о смене программы
        for client in clients:
            await client.send_json({"message": "Program changed", "progNo": progNo, "taktLen": taktLen})
        return JSONResponse(content={"status": "success", "progNo": progNo})
    else:
        raise HTTPException(status_code=400, detail="Invalid input")

async def send_takt_data(websocket: WebSocket):
    global progNo, taktLen, taktNo
    while True:
        # Отправляем данные клиенту
        data = {
            "progNo": progNo,
            "taktLen": taktLen,
            "taktNo": taktNo
        }
        await websocket.send_json(data)

        # Ждем длительность текущего такта перед сменой номера такта
        await asyncio.sleep(taktLen)

        # Обновляем такт
        if taktNo == 14:
            taktNo = 0
        else:
            taktNo += 1

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    clients.append(websocket)
    try:
        await send_takt_data(websocket)
    except WebSocketDisconnect:
        clients.remove(websocket)
