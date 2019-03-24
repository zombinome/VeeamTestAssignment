using System;
using System.IO.Compression;

namespace VeeamTestAssignment
{
    public class Options
    {
        public CompressionMode Mode { get; set; }

        public string InputFilePath { get; set; }

        public string OutputFilePath { get; set; }

        public int ChunkSize { get; set; } = 1024 * 1024;

        public int Workers { get; set; } = Environment.ProcessorCount;
    }
}