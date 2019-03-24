using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace VeeamTestAssignment.Compression
{
    internal class UnarchiveOperation : Operation
    {
        private int chunkSize;

        public UnarchiveOperation(Stream source, Stream dest, int workersCount, CancellationToken cancellationToken)
            : base(source, dest, workersCount, cancellationToken)
        {
        }

        public override void Run()
        {
            this.chunkSize = source.ReadInt32();
            base.Run();
        }

        protected override Chunk ReadChunk(long index)
        {
            long chunkPosition = this.source.Position;

            long chunkIndex = this.source.ReadInt64();
            int chunkSize = this.source.ReadInt32();

            long currentFilePosition = source.Position;

            byte[] buffer = new byte[chunkSize];
            int readBytes = this.source.Read(buffer, 0, chunkSize);
            Chunk chunk = null;
            if (readBytes > 0)
            {
                chunk = new Chunk(chunkIndex, buffer, readBytes);
                this.readChunks.Add(chunk);
                source.Seek(currentFilePosition + readBytes, SeekOrigin.Begin);
            }

            return chunk;
        }

        protected override Chunk TransformChunk(Chunk chunk)
        {
            using (var memoryStream = new MemoryStream(chunk.BufferContent, 0, chunk.Size))
            using (var gZip = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                var buffer = new byte[this.chunkSize];
                int bytesRead = gZip.Read(buffer, 0, this.chunkSize);
                return new Chunk(chunk.Index, buffer, bytesRead);
            }
        }

        protected override void WriteChunk(Chunk chunk)
        {
            this.dest.Seek(this.chunkSize * chunk.Index, SeekOrigin.Begin);
            this.dest.Write(chunk.BufferContent, 0, chunk.Size);
        }
    }
}
