import pandas as pd
import sys

DEFAULT_COUNT = 10

def main():
    head_count = extract_head_count(sys.argv)

    if not isinstance(head_count, int):
        print('Head count has to be an integer.')
        return
    
    df = pd.read_csv('images.csv')
    df = df.head(head_count).to_csv('images_head.csv', index=False)

def extract_head_count(args: list[str]):
    if len(args) < 2:
        return DEFAULT_COUNT
    
    head_count = args[1] or DEFAULT_COUNT

    if not isinstance(head_count, int):
        raise RuntimeError('Head count has to be an integer.')
    
    return head_count

if __name__ == '__main__':
    main()