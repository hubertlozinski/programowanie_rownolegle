import time
from multiprocessing import Pool
from PIL import Image, ImageOps
import numpy as np
import matplotlib

def edge(image):
    image = image
    mask = [[-1, 0, 1],
                [-2, 0, 2],
                [-1, 0, 1]]
    kernel_size = len(mask[0])
    height, width = image.shape
    new_image = np.zeros_like(image)
    for i in range(height - kernel_size + 1):
        for j in range(width - kernel_size + 1):
            values = image[i:i+kernel_size, j:j+kernel_size]
            new_image[i, j] = np.sum(values * mask)
    new_image = np.abs(new_image)
    return new_image

def split_image(image, n):
    image_quaters = []
    for i in range(n):
        image_quaters.append(np.array(image.crop((i * image.width // n, 0, (i + 1) * image.width // n, image.height)))/255)
    return image_quaters

def merge_image(image_parts):
    return np.concatenate(image_parts, axis=1)

if __name__ == '__main__':
    matplotlib.use('TkAgg')
    img = Image.open("DSC00012.jpg")

    split = 4
    image = ImageOps.grayscale(img)


    image_quaters = split_image(image, split)
    image_quaters_cp = split_image(image, split)

    start = time.time()
    with Pool(split) as p:
        results =p.map(edge, image_quaters)

    full_image = merge_image(results)
    end = time.time()
    print(end - start)
    Image.fromarray((full_image * 255).astype(np.uint8)).show()


    results_cp = []
    start = time.time()
    for i in range(split):
        results_cp.append(edge(image_quaters_cp[i]))

    full_image_cp = merge_image(results_cp)
    end = time.time()
    print(end-start)
    Image.fromarray((full_image_cp * 255).astype(np.uint8)).show()