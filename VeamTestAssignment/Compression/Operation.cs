using System;
using System.IO;
using System.Threading;
using VeeamTestAssignment.Collections;

namespace VeeamTestAssignment.Compression
{
    internal abstract class Operation: IDisposable
    {
        private readonly int workersCount;
        private int activeWorkers;

        protected readonly Stream source;
        protected readonly Stream dest;
        protected readonly CancellationToken cancellationToken;

        protected readonly BlockingQueue<Chunk> readChunks;
        protected readonly BlockingQueue<Chunk> chunksToWrite;

        protected Operation(Stream source, Stream dest, int workersCount, CancellationToken cancellationToken)
        {
            this.source = source;
            this.dest = dest;
            this.workersCount = workersCount;
            this.cancellationToken = cancellationToken;

            this.readChunks = new BlockingQueue<Chunk>(workersCount);
            this.chunksToWrite = new BlockingQueue<Chunk>(workersCount);
        }

        protected abstract Chunk ReadChunk(long index);

        protected abstract void WriteChunk(Chunk chunk);

        protected abstract Chunk TransformChunk(Chunk chunk);

        public virtual void Run()
        {
            this.activeWorkers = this.workersCount;
            
            var workerThreads = new Thread[this.workersCount];
            for (int i = 0; i < this.workersCount; i++)
            {
                workerThreads[i] = new Thread(this.Worker);
                workerThreads[i].Name = $"ArhiverOperation::Worker[{i}]";
                workerThreads[i].Start();
            }

            var readThread = new Thread(this.SourceStreamReader);
            readThread.Name = "ArhiverOperation::SourceStreamReader";
            readThread.Start();


            this.DestStreamWriter();
        }

        public void Dispose()
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                while (this.chunksToWrite.Count > 0)
                {
                    if (this.chunksToWrite.TryTake(out Chunk chunk, 10) && chunk.StreamContent != null)
                    {
                        chunk.StreamContent.Dispose();
                    }
                }
            }
        }

        private void SourceStreamReader()
        {
            long index = 0;
            Chunk chunk;
            while (!cancellationToken.IsCancellationRequested && (chunk = this.ReadChunk(index)) != null)
            {
                this.readChunks.Add(chunk);
                index++;
            }

            this.readChunks.CompleteAdding();
        }

        private void DestStreamWriter()
        {
            while (!this.chunksToWrite.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                if (this.chunksToWrite.TryTake(out Chunk chunk, 10))
                {
                    this.WriteChunk(chunk);
                }
            }
        }

        private void Worker()
        {
            while (!this.readChunks.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                Chunk chunk = null;
                if (this.readChunks.TryTake(out chunk, 10))
                {
                    chunk = this.TransformChunk(chunk);
                    this.chunksToWrite.Add(chunk);
                }
            }

            Interlocked.Decrement(ref this.activeWorkers);
            if (this.activeWorkers == 0)
            {
                this.chunksToWrite.CompleteAdding();
            }
        }
    }
}
