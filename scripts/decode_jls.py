from PIL import Image
import sys

def main():
    inp = sys.argv[1]
    out = sys.argv[2]

    img = Image.open(inp)

    img.save(out, "PPM")

if __name__ == '__main__':
    main()
