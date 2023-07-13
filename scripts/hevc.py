import subprocess
import pandas as pd
from PIL import Image
import os
import time

# Współczynnik CRF (Constant Rate Factor), dostosuj go do swoich potrzeb
crf = 28


def convert_ppm_to_png(input_image, output_image):
    im = Image.open(input_image)
    im.save(output_image)


def compress_image_to_video(input_image, output_video, crf):
    command = ['ffmpeg', '-y', '-loop', '1', '-i', input_image, '-c:v', 'libx265', '-crf', str(crf), '-t', '1', output_video]
    subprocess.run(command)


def decompress_video_to_image(input_video, output_image):
    command = ['ffmpeg', '-i', input_video, '-vframes', '1', output_image]
    subprocess.run(command)


# Ścieżka do folderu z obrazami
image_folder = r"C:\Users\BUGI\Desktop\INFA\NTWI\proj\NTwI\images"

# Inicjalizacja DataFrame do przechowywania wyników
results_df = pd.DataFrame(columns=['Nazwa obrazu', 'Współczynnik kompresji', 'Czas kompresji',
                                   'Współczynnik dekompresji', 'Czas dekompresji'])

# Przetwarzanie każdego obrazu w folderze
for filename in os.listdir(image_folder):
    if filename.endswith('.ppm'):
        image_path_ppm = os.path.join(image_folder, filename)

        # Konwersja obrazu PPM na PNG
        image_path_png = image_path_ppm[:-4] + '.png'
        convert_ppm_to_png(image_path_ppm, image_path_png)

        # Kompresja obrazu PNG jako video
        output_video = os.path.join(image_folder, filename[:-4] + '.mp4')
        compress_start_time = time.time()
        compress_image_to_video(image_path_png, output_video, crf)
        compress_end_time = time.time()
        compression_time = compress_end_time - compress_start_time

        # Dekompresja video do obrazu
        output_image = os.path.join(image_folder, filename[:-4] + '_decompressed.ppm')
        decompress_start_time = time.time()
        decompress_video_to_image(output_video, output_image)
        decompress_end_time = time.time()
        decompression_time = decompress_end_time - decompress_start_time

        # Obliczenie współczynnika kompresji
        input_size = os.path.getsize(image_path_ppm)
        compressed_size = os.path.getsize(output_video)
        compression_ratio = input_size / compressed_size

        # Obliczenie współczynnika dekompresji
        decompressed_size = os.path.getsize(output_image)
        decompression_ratio = input_size / decompressed_size

        # Dodanie wyników do DataFrame
        results_df = results_df._append({'Nazwa obrazu': filename, 'Współczynnik kompresji': compression_ratio,
                                        'Czas kompresji': compression_time,
                                        'Współczynnik dekompresji': decompression_ratio,
                                        'Czas dekompresji': decompression_time}, ignore_index=True)


# Obliczenie średnich wartości
avg_compression_ratio = results_df['Współczynnik kompresji'].mean()
avg_compression_time = results_df['Czas kompresji'].mean()
avg_decompression_ratio = results_df['Współczynnik dekompresji'].mean()
avg_decompression_time = results_df['Czas dekompresji'].mean()

# Zapis wyników do pliku Excel
results_df.to_excel('wyniki.xlsx', index=False)

# Wypisanie średnich wartości do sprawozdania
print('Średni współczynnik kompresji:', avg_compression_ratio)
print('Średni czas kompresji:', avg_compression_time)
print('Średni współczynnik dekompresji:', avg_decompression_ratio)
print('Średni czas dekompresji:', avg_decompression_time)
