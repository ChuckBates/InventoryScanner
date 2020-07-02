import urllib.request
import os
from PIL import Image
from pathlib import Path

def download_image(barcode, image_url):
    file_path = f'/home/pi/InventoryScanner/src/images/{barcode}.jpeg'
    download_response = {}
    try:
        urllib.request.urlretrieve(image_url, file_path)
        new_image_url = convert_to_png(file_path)
        download_response['successful'] = True
        download_response['image_path'] = new_image_url
    except:
        download_response['successful'] = False

    return download_response

def convert_to_png(image_file):
    png_image_file_parts = str(image_file).split('.')
    if png_image_file_parts[1] == 'png':
        return image_file

    png_image_file = png_image_file_parts[0] + '.png'
    if Path(png_image_file).is_file():
        return png_image_file
    image = Image.open(image_file)    
    image.save(png_image_file, 'PNG')

    os.remove(image_file)
    return png_image_file
