"""" """
from base64 import standard_b64encode as encode
from sys import argv
import numpy as np
import cv2

def serialize(self) -> str:
    """ Serializes the bitmap to a string """
    return f"BMP{self.width}x{self.height}:{encode(self.data).decode('ascii')}===="

def color_to_int(color: np.ndarray) -> int:
    """ Converts a color to an integer """
    return (color[0] << 16) + (color[1] << 8) + color[2]

def load_bmp(path):
    """Loads a bitmap image from a file.
    Returns a tuple containing the image data and the image size.
    """
    img = cv2.imread(path)
    height, width, _channels = img.shape
    data = []
    for i in range(height):
        data.append([])
        for j in range(width):
            # data.append(f"{{{img[i, j, 0]}, {img[i, j, 1]}, {img[i, j, 2]}}}")
            # data[-1].append(f"{{{img[i, j, 1]}, {img[i, j, 0]}, {img[i, j, 2]}}}")
            data[-1].append(f"0x{color_to_int(img[i, j]):06x}")
    nl = "\n"
    return f"""#pragma once
#include "color.hpp"
const ChristmasClock::Color_t BMP{width}x{height}[{width*height}] = {{{nl}    {("," + nl + "    ").join(map(lambda l: ", ".join(l), data))}{nl}}};
    """


def main(path):
    """ main entry point """
    bmp = load_bmp(path)
    print(bmp)

if __name__ == "__main__":
    if(len(argv) == 2):
        main(argv[1])
    else:
        print("Usage: python mock_image_converter.py <path to image>")
