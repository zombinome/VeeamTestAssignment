using System.IO;

namespace VeeamTestAssignment.Compression
{
    internal class Chunk
    {
        public readonly long Index;

        public readonly byte[] BufferContent;

        public readonly MemoryStream StreamContent;

        public readonly int Size;

        public Chunk(long index, byte[] content, int size)
        {
            this.Index = index;
            this.BufferContent = content;
            this.StreamContent = null;
            this.Size = size;
        }

        public Chunk(long index, MemoryStream content)
        {
            this.Index = index;
            this.BufferContent = null;
            this.StreamContent = content;
            this.Size = (int)(content.Length);
        }
    }
}