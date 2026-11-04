import pickle
def receive_all(sock):
    length = int.from_bytes(sock.recv(4), byteorder='big') 
    data = b''
    while len(data) < length:
        packet = sock.recv(4096)
        if not packet:
            break
        data += packet
    return pickle.loads(data)