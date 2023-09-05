from PIL import Image
import pillow_jpls as _
import sys

def main():
    inp = sys.argv[1]
    out = sys.argv[2]

    img = Image.open(inp)

    options = {
        'near_lossless': 0,
        'interleave': 'none',
        'bits_per_component': 8,
    }

    img.save(out, "JPEG-LS", **options)

if __name__ == '__main__':
    main()