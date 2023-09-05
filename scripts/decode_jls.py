from PIL import Image
import pillow_jpls as _
import sys

def main():
    inp = sys.argv[1]
    out = sys.argv[2]

    img = Image.open(inp, formats=("JPEG-LS",))

    img.save(out, "PPM")

if __name__ == '__main__':
    main()
