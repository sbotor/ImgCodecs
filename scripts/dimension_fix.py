from PIL import Image
import os
from pathlib import Path
import pandas as pd

IMG_DIR = 'images_raw'
TARGET_DIR = 'images'
LIST_PATH = 'images_no_dims.csv'
LIST_TARGET_PATH = 'images.csv'
ALLOWED_EXTENSIONS = ('.ppm')

SILENT = False

def convert_img(path: Path):
    img = Image.open(path)

    width = img.width if img.width % 2 == 0 else img.width - 1
    height = img.height if img.height % 2 == 0 else img.height - 1
    box = (0, 0, width, height)

    converted = img.crop(box)

    if not SILENT:
        resized = width != img.width or height != img.height
        marker = '*' if resized else '-'
        print(f'{marker} {path}: ({img.width}, {img.height}) -> ({width}, {height})')

    output_path = Path(TARGET_DIR, path.name)
    converted.save(output_path)

    return width, height

def main():
    if not os.path.exists(TARGET_DIR):
        os.makedirs(TARGET_DIR)
        
    df = pd.read_csv(LIST_PATH)
    df['width'] = 0
    df['height'] = 0

    for dir in os.listdir(IMG_DIR):
        path = Path(IMG_DIR, dir)
        
        if path.is_file() and path.suffix in ALLOWED_EXTENSIONS:
            width, height = convert_img(path)

            df.loc[df['name'] == path.name, ['width', 'height']] = (width, height)

    df.to_csv(LIST_TARGET_PATH, index=False)

if __name__ == '__main__':
    main()