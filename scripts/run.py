from datetime import datetime
import subprocess

PROJ_PATH = 'src/ImgCodecs/ImgCodecs.csproj'

BENCH_TYPES = ('JpegLs', 'Jpeg2000', 'JpegXl', 'Flif', 'Hevc', 'Vvc')
RUN_COUNT = 10
WARMUP_COUNT = 5
BATCH_SIZE = 5
TIMEOUT = 30_000
LIST_PATH = 'images_head.csv'
IMG_DIR_PATH = 'images'
TEMP_DIR_PATH = 'temp'
RESULTS_PATH = 'results.csv'
ROOT_PATH = '.'
TEMP_CLEANUP = 'DeleteAll'
THREADS = '0'

def build():
    params = ('dotnet', 'build', PROJ_PATH, '-c', 'Release')
    subprocess.run(params)

def run_single(bench_type, filename_prefix = None):
    res_path = filename_prefix + RESULTS_PATH if filename_prefix else RESULTS_PATH
    
    params = ('dotnet', 'run',
            '--project', PROJ_PATH,
            '-c', 'Release', '--',
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

def run_all():
    now = datetime.now().strftime('%Y-%m-%d-%H-%M-%S')
    for bench_type in BENCH_TYPES:
        run_single(bench_type, f'{now}_{bench_type}_')

def main():
    build()

    run_single(BENCH_TYPES[0])
    #run_all()

if __name__ == '__main__':
    main()