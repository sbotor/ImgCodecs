﻿namespace ImgCodecs.Configuration;

public class BenchmarkSettings
{
    public int RunCount { get; set; } = 10;
    public int WarmupCount { get; set; } = 10;
    public int ImageBatchSize { get; set; } = 5;
    public BenchmarkType BenchmarkType { get; set; }
    public int Threads { get; set; }
}

public enum BenchmarkType
{
    JpegLs,
    Jpeg2000,
    JpegXl,
    Flif,
    Hevc,
    HevcLossless,
    Vvc
}