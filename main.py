import tkinter as tk
from tkinter import messagebox, scrolledtext
import socket
import threading
from signalrcore.hub_connection_builder import HubConnectionBuilder
import asyncio
import websockets
import uuid

# Функция для добавления записи в лог (а также для локального кэширования можно расширить этот функционал)
def log_message(text):
    text_area.configure(state="normal")
    text_area.insert(tk.END, text + "\n")
    text_area.see(tk.END)
    text_area.configure(state="disabled")
    # Здесь можно добавить запись в локальный файл/базу для кэширования истории

# Функции для работы по TCP
def tcp_connect(ip, port, session_id):
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect((ip, int(port)))
        # Отправляем уникальный идентификатор сессии
        s.sendall(session_id.encode())
        data = s.recv(1024)
        log_message(f"TCP от {ip}:{port} получил: {data.decode()}")
        s.close()
    except Exception as e:
        log_message(f"Ошибка TCP подключения к {ip}:{port}: {e}")

def on_tcp_connect():
    server = tcp_server_entry.get().strip()
    if server:
        try:
            ip, port = server.split(":")
            session_id = str(uuid.uuid4())
            threading.Thread(target=tcp_connect, args=(ip, port, session_id), daemon=True).start()
        except Exception as e:
            messagebox.showerror("Ошибка", f"Неверный формат сервера: {server}\nОжидается формат IP:port\n{e}")

# Функция для прослушивания UDP-сообщений
def udp_listener():
    udp_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_port = 9100  # порт, на который сервер отправляет реальные UDP данные
    try:
        udp_sock.bind(('0.0.0.0', udp_port))
    except Exception as e:
        log_message(f"Ошибка привязки UDP: {e}")
        return
    log_message(f"UDP клиент слушает на порту {udp_port}")
    while True:
        try:
            data, addr = udp_sock.recvfrom(1024)
            log_message(f"UDP от {addr}: {data.decode()}")
        except Exception as e:
            log_message(f"Ошибка UDP: {e}")
            break

# Асинхронная функция WebSocket‑клиента (SignalR‑хаб)
async def websocket_handler():
    uri = "ws://localhost:5258/newObjectHub"  # адрес вашего SignalR-хаба на бэкенде
    try:
        async with websockets.connect(uri) as websocket:
            # Можно отправить приветственное сообщение, если требуется
            await websocket.send("Привет от Tkinter клиента!")
            while True:
                message = await websocket.recv()
                log_message(f"WebSocket: {message}")
    except Exception as e:
        log_message(f"Ошибка WebSocket: {e}")

# Функция для запуска асинхронного цикла WebSocket-клиента в отдельном потоке
def run_websocket_client():
    asyncio.run(websocket_handler())


def start_signalr_client():
    hub_connection = HubConnectionBuilder() \
        .with_url("http://localhost:5258/newObjectHub") \
        .configure_logging(1) \
        .build()

    # Подписываемся на методы хаба
    hub_connection.on("ReceiveNewObject", lambda message: log_message(f"SignalR Новый объект: {message}"))
    hub_connection.on("ReceiveEvent", lambda message: log_message(f"SignalR Событие: {message}"))
    hub_connection.on("ReceiveUdpData", lambda message: log_message(f"SignalR UDP данные: {message}"))

    hub_connection.start()
    # Если хотите отправлять сообщения, можно вызывать hub_connection.send(...)

# Создание основного окна Tkinter
root = tk.Tk()
root.title("CyberAstronomy – Клиент на Python (Tkinter)")

# --- Интерфейс для TCP ---
tcp_frame = tk.Frame(root)
tcp_frame.pack(pady=10)
tk.Label(tcp_frame, text="TCP сервер (IP:port):").pack(side=tk.LEFT)
tcp_server_entry = tk.Entry(tcp_frame, width=20)
tcp_server_entry.pack(side=tk.LEFT, padx=5)
tcp_connect_btn = tk.Button(tcp_frame, text="Подключиться по TCP", command=on_tcp_connect)
tcp_connect_btn.pack(side=tk.LEFT)

# --- Текстовая область для логов ---
text_area = scrolledtext.ScrolledText(root, width=80, height=20, state="disabled")
text_area.pack(padx=10, pady=10)

# --- Запуск UDP-слушателя в отдельном потоке ---
udp_thread = threading.Thread(target=udp_listener, daemon=True)
udp_thread.start()

# # --- Запуск WebSocket-клиента в отдельном потоке ---
# ws_thread = threading.Thread(target=run_websocket_client, daemon=True)
# ws_thread.start()

signalr_thread = threading.Thread(target=start_signalr_client, daemon=True)
signalr_thread.start()

# Запуск цикла обработки событий Tkinter
root.mainloop()
