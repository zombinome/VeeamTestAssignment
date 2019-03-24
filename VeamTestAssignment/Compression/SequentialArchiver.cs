using System;
using System.IO;
using System.IO.Compression;
using VeeamTestAssignment.Compression;

namespace VeeamTestAssignment
{
    public class SequentialArchiver
    {
        private int defaultChunkSize = 1024 * 1024;

        public void Compress(Stream source, Stream target)
        {
            Console.WriteLine($"Write chunk size {defaultChunkSize} at position: {target.Position}");
            target.WriteInt32(defaultChunkSize);
            Chunk chunk = null; int index = 0;
            while ((chunk = ReadChunk(source, defaultChunkSize)) != null)
            {
                index++;
                using (var memoryStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
                    {
                        zipStream.Write(chunk.Bytes, 0, chunk.Size);
                        zipStream.Flush();
                    }

                    Console.WriteLine($"Write chunk #{index} size ({memoryStream.Position}) at {target.Position}");
                    target.WriteInt32((int)memoryStream.Position);

                    Console.WriteLine($"Write chunk #{index} bytes at {target.Position}");
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(target);
                }
            }
        }

        public void Decompress(Stream source, Stream target)
        {
            int uncompressedChunkSize = source.ReadInt32();
            Console.WriteLine($"Read uncomressed chunks size {uncompressedChunkSize} at position: {source.Position - 4}");

            byte[] buffer = new byte[uncompressedChunkSize];

            int index = 0;
             while (source.Position < source.Length - 1)
            {
                index++;
                int currentChunkSize = source.ReadInt32();
                Console.WriteLine($"Read chunk #{index} size ({currentChunkSize}) at {source.Position - 4}");
                Console.WriteLine($"Read chunk #{index} bytes at {source.Position}");
                long currentFilePosition = source.Position;
                using (var zipStream = new GZipStream(source, CompressionMode.Decompress, true))
                {
                    int readBytes = zipStream.Read(buffer, 0, uncompressedChunkSize);
                    if (readBytes == 0)
                    {
                        break;
                    }

                    target.Write(buffer, 0, readBytes);
                    source.Seek(currentFilePosition + currentChunkSize, SeekOrigin.Begin);
                }
            }
        }

        private static Chunk ReadChunk(Stream source, int chunkSize)
        {
            var buffer = new byte[chunkSize];
            var readBytes = source.Read(buffer, 0, chunkSize);
            if (readBytes == 0)
            {
                return null;
            }

            return new Chunk(0, buffer, readBytes, false);
        }

        private static Chunk ReadChunk(Stream source)
        {
            var chunkSize = source.ReadInt32();
            var buffer = new byte[chunkSize];

            return new Chunk(0, buffer, chunkSize, true);
        }

        private static void WriteChunk(Stream target, Chunk chunk)
        {
            if (chunk.Compressed)
            {
                target.WriteInt32(chunk.Size);
            }

            target.Write(chunk.Bytes, 0, chunk.Size);
        }

        private class Chunk
        {
            public readonly byte[] Bytes;

            public readonly bool Compressed;

            public readonly int Size;

            public readonly long Index;

            public Chunk(long index, byte[] bytes, int count, bool compressed)
            {
                this.Index = index;
                this.Bytes = bytes;
                this.Size = count;
                this.Compressed = compressed;
            }
        }
    }
}
