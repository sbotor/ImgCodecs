﻿namespace ImgCodecs.Configuration;

public class DirectorySettings
{
    public string ImagesDirectoryPath { get; set; } = "./images";
    public string ImageListPath { get; set; } = "./images.csv";
    public string TempDirectoryPath { get; set; } = "./temp";
    public string ResultsPath { get; set; } = "./results.csv";
}