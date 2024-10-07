import time

from modules.events import Event


event1 = Event()
event2 = Event()
event3 = Event()
event4 = Event()
event5 = Event()
event6 = Event()

class TrafficLightCycle:
    def __init__(self):
        self.events = [event1, event2, event3, event4, event5, event6]
        self.cycle_duration = 15  # Длительность каждого ивента в секундах

    def run_cycle(self):
        while True:
            for i, ev in enumerate(self.events):
                print(f"Ивент {i+1} активен")
                ev.set()  # Активируем ивент
                time.sleep(self.cycle_duration)
                ev.clear()  # Очищаем ивент
                print(f"Ивент {i+1} завершен")