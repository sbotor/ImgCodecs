from datetime import datetime
import subprocess
import sys

PROJ_PATH = 'src/ImgCodecs/ImgCodecs.csproj'

BENCH_TYPES = ('JpegLs', 'Jpeg2000', 'JpegXl', 'Flif', 'Hevc', 'Vvc')
RUN_COUNT = 10
WARMUP_COUNT = 5
BATCH_SIZE = 50
TIMEOUT = 30_000
LIST_PATH = 'images.csv'
IMG_DIR_PATH = 'images'
TEMP_DIR_PATH = 'temp'
RESULTS_PATH = 'results.csv'
ROOT_PATH = '.'
TEMP_CLEANUP = 'DeleteAll'
THREADS = '0'

_BENCH_TYPES_LOWERCASE = tuple((x.lower() for x in BENCH_TYPES))

def resolve_bench_type():
    if len(sys.argv) < 2:
        return None
    
    value = sys.argv[1]

    if value.isnumeric():
        idx = int(value)
        if idx < 0 or idx >= len(BENCH_TYPES):
            raise ValueError(f'Invalid benchmark type index {idx}.')
    else:
        try:
            idx = _BENCH_TYPES_LOWERCASE.index(value.lower())
        except ValueError:
            raise ValueError(f'Invalid benchmark type {value}.')        
        
    return BENCH_TYPES[idx]

def build():
    params = ('dotnet', 'build', PROJ_PATH, '-c', 'Release')
    subprocess.run(params)

def results_filename(now: str, bench_type: str):
    prefix = f'{now}_{bench_type}_'
    return prefix + RESULTS_PATH

def run_single(bench_type, now: str):    
    res_path = results_filename(bench_type, now)
    
    params = ('dotnet', 'run',
            '--project', PROJ_PATH,
            '-c', 'Release',
            '--',
            bench_type,
            '--count', str(RUN_COUNT),
            '--warmup-count', str(WARMUP_COUNT),
            '--batch-size', str(BATCH_SIZE),
            '--timeout', str(TIMEOUT),
            '--list', LIST_PATH,
            '--images', IMG_DIR_PATH,
            '--temp', TEMP_DIR_PATH,
            '--output', res_path,
            '--root', ROOT_PATH,
            '--temp-cleanup', TEMP_CLEANUP,
            '--threads', THREADS)
    
    subprocess.run(params)

def run_all(now: str):
    for bench_type in BENCH_TYPES:
        run_single(bench_type, now)

def main():
    bench_type = resolve_bench_type()
    build()

    now = datetime.now().strftime('%Y-%m-%d-%H-%M-%S')

    if bench_type:
        run_single(bench_type, now)
    else:
        run_all(now)

if __name__ == '__main__':
    main()