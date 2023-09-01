from PIL import Image
import os
from pathlib import Path

IMG_DIR = 'images_raw'
TARGET_DIR = 'images'
ALLOWED_EXTENSIONS = ('.ppm')

SILENT = False

def convert_img(path: Path):
    img = Image.open(path)

    width = img.width if img.width % 2 == 0 else img.width - 1
    height = img.height if img.height % 2 == 0 else img.height - 1
    box = (0, 0, height, width)

    converted = img.crop(box)

    if not SILENT:
        resized = width != img.width or height != img.height
        marker = '*' if resized else '-'
        print(f'{marker} {path}: ({img.width}, {img.height}) -> ({width}, {height})')

    output_path = Path(TARGET_DIR, path.name)
    converted.save(output_path)

def main():
    if not os.path.exists(TARGET_DIR):
        os.makedirs(TARGET_DIR)

    for dir in os.listdir(IMG_DIR):
        path = Path(IMG_DIR, dir)
        
        if path.is_file() and path.suffix in ALLOWED_EXTENSIONS:
            convert_img(path)

if __name__ == '__main__':
    main()