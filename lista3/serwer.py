from PIL import Image, ImageOps
import socket
from receive import receive_all
from send import send_all
from zad1 import split_image, merge_image
import numpy as np
import sys

def server_main(image_path, n_clients):
    image = Image.open(image_path)
    image = ImageOps.grayscale(image)
    fragments = split_image(image, n_clients)
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('10.133.101.128', 50000))
    server_socket.listen(n_clients)
    print("Serwer nasłuchuje...")
    processed_fragments = []
    for i in range(n_clients):
        client_socket, client_adress = server_socket.accept()
        print(f"Połączono z klientem {i+1}: {client_adress}")
        send_all(client_socket, fragments[i])
        processed_fragment = receive_all(client_socket)
        processed_fragments.append(processed_fragment)
        client_socket.close()
    merged_img = merge_image(processed_fragments)
    clipped_img = np.clip(merged_img, 0, 1)
    result_image = Image.fromarray((clipped_img * 255).astype(np.uint8))
    result_image.save("processed_image.png")
    print("Obraz przetworzony zapisany jako processed_image.png")
if __name__ == '__main__':
    server_main("DSC00012.jpg", 2)