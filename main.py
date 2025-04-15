import tkinter as tk
from tkinter import messagebox, scrolledtext
import socket
import threading
from signalrcore.hub_connection_builder import HubConnectionBuilder
import asyncio
import websockets
import uuid


# Функция для потокобезопасного добавления записей в лог
def log_message(text, log_type):
    def update_log():
        if log_type == 'tcp':
            text_area = tcp_text_area
        elif log_type == 'udp':
            text_area = udp_text_area
        elif log_type == 'websocket':
            text_area = websocket_text_area
        else:
            return

        text_area.configure(state="normal")
        text_area.insert(tk.END, text + "\n")
        text_area.see(tk.END)
        text_area.configure(state="disabled")

    root.after(0, update_log)


# Функции для работы по TCP
def tcp_connect(ip, port, session_id):
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect((ip, int(port)))
        s.sendall(session_id.encode())
        data = s.recv(1024)
        log_message(f"TCP от {ip}:{port} получил: {data.decode()}", 'tcp')
        s.close()
    except Exception as e:
        log_message(f"Ошибка TCP подключения к {ip}:{port}: {e}", 'tcp')


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
    global udp_sock
    udp_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_port = 9000
    try:
        udp_sock.bind(('0.0.0.0', udp_port))
    except Exception as e:
        log_message(f"Ошибка привязки UDP: {e}", 'udp')
        return
    log_message(f"UDP клиент слушает на порту {udp_port}", 'udp')
    while True:
        try:
            data, addr = udp_sock.recvfrom(4096)
            log_message(f"UDP от {addr}: {data.decode()}", 'udp')
        except Exception as e:
            log_message(f"Ошибка UDP: {e}", 'udp')
            break


# Асинхронная функция WebSocket-клиента
async def websocket_handler():
    uri = "ws://localhost:5258/newObjectHub"
    try:
        async with websockets.connect(uri) as websocket:
            await websocket.send("Привет от Tkinter клиента!")
            while True:
                message = await websocket.recv()
                log_message(f"WebSocket: {message}", 'websocket')
    except Exception as e:
        log_message(f"Ошибка WebSocket: {e}", 'websocket')


def run_websocket_client():
    asyncio.run(websocket_handler())


# SignalR клиент
def start_signalr_client():
    hub_connection = HubConnectionBuilder() \
        .with_url("http://localhost:5258/newObjectHub") \
        .configure_logging(1) \
        .build()

    hub_connection.on("ReceiveNewObject", lambda message: log_message(f"Новый объект: {message}", 'websocket'))
    hub_connection.on("ReceiveEvent", lambda message: log_message(f"Событие: {message}", 'websocket'))
    hub_connection.on("ReceiveUdpData", lambda message: log_message(f"UDP данные: {message}", 'websocket'))

    hub_connection.start()


# Создание GUI
root = tk.Tk()
root.title("CyberAstronomy – Клиент на Python (Tkinter)")

# TCP подключение
tcp_frame = tk.Frame(root)
tcp_frame.pack(pady=10)
tk.Label(tcp_frame, text="TCP сервер (IP:port):").pack(side=tk.LEFT)
tcp_server_entry = tk.Entry(tcp_frame, width=20)
tcp_server_entry.pack(side=tk.LEFT, padx=5)
tcp_connect_btn = tk.Button(tcp_frame, text="Подключиться по TCP", command=on_tcp_connect)
tcp_connect_btn.pack(side=tk.LEFT)

# Панель с логами
logs_frame = tk.Frame(root)
logs_frame.pack(padx=10, pady=10, fill=tk.BOTH, expand=True)

# TCP логи
tcp_log_frame = tk.Frame(logs_frame)
tcp_log_frame.grid(row=0, column=0, sticky="nsew", padx=2, pady=2)
tk.Label(tcp_log_frame, text="TCP Логи").pack()
tcp_text_area = scrolledtext.ScrolledText(tcp_log_frame, state="disabled")
tcp_text_area.pack(fill=tk.BOTH, expand=True)

# UDP логи
udp_log_frame = tk.Frame(logs_frame)
udp_log_frame.grid(row=0, column=1, sticky="nsew", padx=2, pady=2)
tk.Label(udp_log_frame, text="UDP Логи").pack()
udp_text_area = scrolledtext.ScrolledText(udp_log_frame, state="disabled")
udp_text_area.pack(fill=tk.BOTH, expand=True)

# WebSocket логи
websocket_log_frame = tk.Frame(logs_frame)
websocket_log_frame.grid(row=0, column=2, sticky="nsew", padx=2, pady=2)
tk.Label(websocket_log_frame, text="WebSocket Логи").pack()
websocket_text_area = scrolledtext.ScrolledText(websocket_log_frame, state="disabled")
websocket_text_area.pack(fill=tk.BOTH, expand=True)

logs_frame.grid_columnconfigure(0, weight=1)
logs_frame.grid_columnconfigure(1, weight=1)
logs_frame.grid_columnconfigure(2, weight=1)
logs_frame.grid_rowconfigure(0, weight=1)

# Запуск потоков
udp_thread = threading.Thread(target=udp_listener, daemon=True)
udp_thread.start()

signalr_thread = threading.Thread(target=start_signalr_client, daemon=True)
signalr_thread.start()

root.mainloop()