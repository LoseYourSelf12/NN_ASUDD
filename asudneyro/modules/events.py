class Event:
    def __init__(self, event_name="", event_type="", event_duration=0, event_state=False):
        self.name = event_name
        self.type = event_type  
        self.duration = event_duration
        self.state = event_state
    
    def is_set(self):
        return self.state
    
    def set(self):
        self.state = True
    
    def clear(self):
        self.state = False

# # Инициализация ивентов
# detect_event = Event()
# stop_event = Event()