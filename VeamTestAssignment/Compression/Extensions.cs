using System.IO;

namespace VeeamTestAssignment.Compression
{
    internal static class Extensions
    {
        public static void WriteInt32(this Stream target, int value)
        {
            var buffer = new byte[4];
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                buffer[i] = (byte)(value & 0xFF);
                value = value / 256;
            }

            target.Write(buffer, 0, buffer.Length);
        }

        public static int ReadInt32(this Stream target)
        {
            var buffer = new byte[4];
            int readBytes = target.Read(buffer, 0, buffer.Length);

            int result = 0;
            for (int i = 0; i < readBytes; i++)
            {
                result = result * 256 + buffer[i];
            }

            return result;
        }

        public static void WriteInt64(this Stream target, long value)
        {
            var buffer = new byte[8];
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                buffer[i] = (byte)(value & 0xFF);
                value = value / 256;
            }

            target.Write(buffer, 0, buffer.Length);
        }

        public static long ReadInt64(this Stream target)
        {
            var buffer = new byte[8];
            int readBytes = target.Read(buffer, 0, buffer.Length);

            long result = 0;
            for (int i = 0; i < readBytes; i++)
            {
                result = result * 256 + buffer[i];
            }

            return result;
        }
    }
}