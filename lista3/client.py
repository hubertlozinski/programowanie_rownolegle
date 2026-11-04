import socket
import sys

from receive import receive_all
from send import send_all
from zad1 import edge

def client_main():
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect(('10.133.101.128', int(sys.argv[1])))
    fragment = receive_all(client_socket)
    processed_fragment = edge(fragment)
    send_all(client_socket, processed_fragment)
    client_socket.close()
    print("Fragment przetworzony i wysłany z powrotem do serwera")

client_main()