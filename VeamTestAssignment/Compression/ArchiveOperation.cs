using System.IO;
using System.IO.Compression;
using System.Threading;

namespace VeeamTestAssignment.Compression
{
    internal class ArchiveOperation : Operation
    {
        private readonly int chunkSize;

        public ArchiveOperation(Stream source, Stream dest, int workersCount, CancellationToken cancellationToken, int chunkSize)
            : base(source, dest, workersCount, cancellationToken)
        {
            this.chunkSize = chunkSize;
        }

        public override void Run()
        {
            this.dest.WriteInt32(this.chunkSize);
            base.Run();
        }

        /// <summary>
        /// Reads chunk of source file to compress
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns></returns>
        protected override Chunk ReadChunk(long index)
        {
            Chunk chunk = null;

            byte[] buffer = new byte[this.chunkSize];
            int readBytes = this.source.Read(buffer, 0, this.chunkSize);
            if (readBytes > 0)
            {
                chunk = new Chunk(index, buffer, readBytes);
            }

            return chunk;
        }

        /// <summary>
        /// Compresses chunk using <see cref="GZipStream"/>
        /// </summary>
        /// <param name="chunk">Chunk to compress</param>
        /// <returns>Compressed chunk</returns>
        protected override Chunk TransformChunk(Chunk chunk)
        {
            var memoryStream = new MemoryStream();
            using (var gzip = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
            {
                gzip.Write(chunk.BufferContent, 0, chunk.Size);
                gzip.Flush();
            }

            return new Chunk(chunk.Index, memoryStream);
        }

        /// <summary>
        /// Writes compressed chunk to archive
        /// </summary>
        /// <param name="chunk">Chunk to write</param>
        protected override void WriteChunk(Chunk chunk)
        {
            this.dest.WriteInt64(chunk.Index);
            this.dest.WriteInt32(chunk.Size);
            chunk.StreamContent.Seek(0, SeekOrigin.Begin);
            chunk.StreamContent.CopyTo(this.dest);
            chunk.StreamContent.Dispose();
        }
    }
}
