using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace VeeamTestAssignment.Compression
{
    public class ParallelArchiver: IDisposable
    {
        private readonly Stream source;
        private readonly Stream dest;
        private readonly int workersCount;
        private int chunkSize;
        private readonly CompressionMode mode;
        private volatile int activeWorkers;
        private readonly object locker;

        private readonly BlockingCollection<Chunk> readChunks;
        private readonly BlockingCollection<Chunk> chunksToWrite;

        public string Name { get; set; }

        public ParallelArchiver(Stream source, Stream dest, int chunkSize, CompressionMode mode, int? workersCount = null)
        {
            this.source = source;
            this.dest = dest;
            this.workersCount = workersCount ?? Environment.ProcessorCount;
            this.chunkSize = chunkSize;
            this.mode = mode;
            this.readChunks = new BlockingCollection<Chunk>(this.workersCount);
            this.chunksToWrite = new BlockingCollection<Chunk>(this.workersCount);
            this.locker = new object();
        }

        public void Run(CancellationToken cancellationToken)
        {
            ThreadStart readWorker = null;
            Action<CancellationToken> writeWorker = null;
            Func<Chunk, Chunk> chunkTransformer = null;
            this.activeWorkers = this.workersCount;
            if (this.mode == CompressionMode.Compress)
            {
                readWorker = () => this.FileReader(cancellationToken);
                writeWorker = this.ArchiveWriter;

                chunkTransformer = this.ComressChunk;
            }
            else
            {
                readWorker = () => this.ArchiveReader(cancellationToken);
                writeWorker = this.FileWriter;

                chunkTransformer = this.DecompressChunk;
            }

            var workerThreads = new Thread[this.workersCount];
            for (int i = 0; i < this.workersCount; i++)
            {
                workerThreads[i] = new Thread(() => this.Worker(chunkTransformer, cancellationToken));
                workerThreads[i].Name = $"Archiver::Worker[{i}]";
                workerThreads[i].Start();
            }

            var readThread = new Thread(readWorker);
            readThread.Name = "Arhiver::OriginalFileReader";
            readThread.Start();


            writeWorker(cancellationToken);
        }

        private void FileReader(CancellationToken cancellationToken)
        {
            int readBytes;
            long index = 0;
            do
            {
                byte[] buffer = new byte[this.chunkSize];
                readBytes = this.source.Read(buffer, 0, this.chunkSize);
                if (readBytes > 0)
                {
                    var chunk = new Chunk(index, buffer, readBytes);
                    this.readChunks.Add(chunk);
                    index++;
                }
            }
            while (readBytes > 0 && !cancellationToken.IsCancellationRequested);

            this.readChunks.CompleteAdding();
        }

        private void FileWriter(CancellationToken cancellationToken)
        {
            try
            {
                while (!this.chunksToWrite.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    if (this.chunksToWrite.TryTake(out Chunk chunk, 10))
                    {
                        this.dest.Seek(this.chunkSize * chunk.Index, SeekOrigin.Begin);
                        this.dest.Write(chunk.BufferContent, 0, chunk.Size);
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ArchiveReader(CancellationToken cancellationToken)
        {
            this.chunkSize = source.ReadInt32();
            while(this.source.Position < this.source.Length && !cancellationToken.IsCancellationRequested)
            {
                long chunkPosition = this.source.Position;

                long chunkIndex = this.source.ReadInt64();
                int chunkSize = this.source.ReadInt32();

                long currentFilePosition = source.Position;

                byte[] buffer = new byte[chunkSize];
                int readBytes = this.source.Read(buffer, 0, chunkSize);
                if (readBytes > 0)
                {
                    Console.WriteLine($"{this.Name}#R:chunk:{chunkIndex} at {chunkPosition}, length: {chunkSize + 12}");

                    var chunk = new Chunk(chunkIndex, buffer, readBytes);
                    this.readChunks.Add(chunk);
                    source.Seek(currentFilePosition + readBytes, SeekOrigin.Begin);
                }

            }

            this.readChunks.CompleteAdding();
        }

        private void ArchiveWriter(CancellationToken cancellationToken)
        {
            try
            {
                this.dest.WriteInt32(this.chunkSize);
                while (!this.chunksToWrite.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    if (this.chunksToWrite.TryTake(out Chunk chunk))
                    {
                        Console.WriteLine($"{this.Name}#W:chunk:{chunk.Index} at {dest.Position}, length: {chunk.Size + 12}");
                        this.dest.WriteInt64(chunk.Index);
                        this.dest.WriteInt32(chunk.Size);
                        chunk.StreamContent.Seek(0, SeekOrigin.Begin);
                        chunk.StreamContent.CopyTo(this.dest);
                        chunk.StreamContent.Dispose();
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private Chunk ComressChunk(Chunk chunk)
        {
            var memoryStream = new MemoryStream();
            using (var gzip = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
            {
                gzip.Write(chunk.BufferContent, 0, chunk.Size);
                gzip.Flush();
            }

            return new Chunk(chunk.Index, memoryStream);
        }

        private Chunk DecompressChunk(Chunk chunk)
        {
            using (var memoryStream = new MemoryStream(chunk.BufferContent, 0, chunk.Size))
            using (var gZip = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                var buffer = new byte[this.chunkSize];
                int bytesRead = gZip.Read(buffer, 0, this.chunkSize);
                return new Chunk(chunk.Index, buffer, bytesRead);
            }
        }

        private void Worker(Func<Chunk, Chunk> chunkTransformer, CancellationToken cancellationToken)
        {
            try
            {
                while (!this.readChunks.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    if (this.readChunks.TryTake(out Chunk chunk, 10))
                    {
                        chunk = chunkTransformer(chunk);
                        this.chunksToWrite.Add(chunk);
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }

            Interlocked.Decrement(ref this.activeWorkers);
            if (this.activeWorkers == 0)
            {
                this.chunksToWrite.CompleteAdding();
            }
        }

        public void Dispose()
        {
            this.readChunks.Dispose();
            this.chunksToWrite.Dispose();
        }

        private static Chunk CompressChunk(Chunk uncompressedChunk)
        {
            var memoryStream = new MemoryStream();
            using (var gzip = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
            {
                gzip.Write(uncompressedChunk.BufferContent, 0, uncompressedChunk.Size);
                gzip.Flush();
            }

            return new Chunk(uncompressedChunk.Index, memoryStream);
        }

        private static Chunk UncompressChunk(Chunk compressedChunk, int uncompressedChunkSize)
        {
            using (var memoryStream = new MemoryStream(compressedChunk.BufferContent, 0, compressedChunk.Size))
            using (var gZip = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                var buffer = new byte[uncompressedChunkSize];
                int bytesRead = gZip.Read(buffer, 0, uncompressedChunkSize);
                return new Chunk(compressedChunk.Index, buffer, bytesRead);
            }
        }

        private static Chunk ReadUncompressedChunk(Stream source, int chunkSize, int chunkIndex)
        {
            Chunk chunk = null;
            byte[] buffer = new byte[chunkSize];
            int readBytes = source.Read(buffer, 0, chunkSize);
            if (readBytes > 0)
            {
                chunk = new Chunk(chunkIndex, buffer, readBytes);
            }

            return chunk;
        }

        private static Chunk ReadCompressedChunk(Stream source)
        {
            long chunkPosition = source.Position;

            long chunkIndex = source.ReadInt64();
            int chunkSize = source.ReadInt32();

            long currentFilePosition = source.Position;

            byte[] buffer = new byte[chunkSize];
            int readBytes = source.Read(buffer, 0, chunkSize);
            Chunk chunk = null;

            if (readBytes > 0)
            {
                Console.WriteLine($"R#chunk:{chunkIndex} at {chunkPosition}, length: {chunkSize + 12}");

                chunk = new Chunk(chunkIndex, buffer, readBytes);
                source.Seek(currentFilePosition + readBytes, SeekOrigin.Begin);
            }

            return chunk;
        }

        private static void WriteUncompressedChunk(Stream dest, Chunk chunk, int chunkSize)
        {
            dest.Seek(chunkSize * chunk.Index, SeekOrigin.Begin);
            dest.Write(chunk.BufferContent, 0, chunk.Size);
        }

        private static void WriteCompressedChunk(Stream dest, Chunk chunk)
        {

        }
    }
}
