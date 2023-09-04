import pandas as pd
import sys

SOURCE_CSV = 'images.csv'
TARGET_CSV = 'images_head.csv'

DEFAULT_COUNT = 10

def main():
    head_count = extract_head_count(sys.argv)
    
    df = pd.read_csv(SOURCE_CSV)
    df = df.head(head_count).to_csv(TARGET_CSV, index=False)

def extract_head_count(args: list[str]):
    if len(args) < 2:
        return DEFAULT_COUNT
    
    head_count = args[1] or DEFAULT_COUNT

    return int(head_count)

if __name__ == '__main__':
    main()