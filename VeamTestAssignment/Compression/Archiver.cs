using System.IO;
using System.Threading;

namespace VeeamTestAssignment.Compression
{
    public static class Archiver
    {
        public static void Archive(Stream source, Stream dest, int chunkSize, int workersCount, CancellationToken cancellationToken)
        {
            using (var operation = new ArchiveOperation(source, dest, workersCount, cancellationToken, chunkSize))
            {
                operation.Run();
            }
        }

        public static void Unarchive(Stream source, Stream dest, int workersCount, CancellationToken cancellationToken)
        {
            using (var operation = new UnarchiveOperation(source, dest, workersCount, cancellationToken))
            {
                operation.Run();
            }
        }
    }
}