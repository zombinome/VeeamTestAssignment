using System;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Threading;
using VeeamTestAssignment.Compression;

namespace VeeamTestAssignment
{
    class Program
    {
        private const int
            CodeOk = 0,
            CodeFail = 1;

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return CodeFail;
            }

            var programOptionsResult = GetOptions(args);
            if (!programOptionsResult.Success)
            {
                Console.WriteLine(programOptionsResult.Error);
                PrintHelp();
                return CodeFail;
            }

            Options options = programOptionsResult.Value;
            var cts = new CancellationTokenSource();

            var sourceStreamResult = CreateSourceFileStream(options.InputFilePath);
            if (!sourceStreamResult.Success)
            {
                Console.WriteLine(sourceStreamResult.Error);
                return CodeFail;
            }

            var destStreamResult = CreateDestFileStream(options.OutputFilePath);
            if(!destStreamResult.Success)
            {
                Console.WriteLine(destStreamResult.Error);
                sourceStreamResult.Value.Dispose();
                return CodeFail;
            }

            using (var sourceStream = sourceStreamResult.Value)
            using (var destStream = destStreamResult.Value)
            {
                if (options.Mode == CompressionMode.Compress)
                {
                    Archiver.Archive(sourceStream, destStream, options.ChunkSize, options.Workers, cts.Token);
                }
                else
                {
                    Archiver.Unarchive(sourceStream, destStream, options.Workers, cts.Token);
                }
            }

            return CodeOk;
        }

        private static Result<Options> GetOptions(string[] args)
        {
            if (args.Length < 3)
            {
                return new Result<Options>(Messages.Error_InvalidProgramArguments);
            }
            
            if (!Enum.TryParse(args[0], true, out CompressionMode mode))
            {
                return new Result<Options>(Messages.Error_UnrecognizedCommand);
            }

            if (!File.Exists(args[1]))
            {
                return new Result<Options>(Messages.Error_InvalidInputFilePath);
            }

            var programArgs = new Options
            {
                Mode = mode,
                InputFilePath = args[1],
                OutputFilePath = args[2],
            };

            for (int i = 3; i < args.Length - 1; i+= 2)
            {
                string name = args[i].ToUpperInvariant();
                string value = args[i + 1];

                switch(name)
                {
                    case "CHUNKSIZE":
                        if (!int.TryParse(value, out int chunkSize) || chunkSize < 1)
                        {
                            return new Result<Options>(Messages.Error_InvalidChunkSizeValue);
                        }

                        programArgs.ChunkSize = chunkSize;
                        break;
                    case "WORKERS":
                        if (!int.TryParse(value, out int workersCount) || workersCount < 1)
                        {
                            return new Result<Options>(Messages.Error_InvalidChunkSizeValue);
                        }

                        programArgs.Workers = workersCount;
                        break;
                    default:
                        return new Result<Options>(Messages.Error_UnrecognizedParameter);
                }
            }

            return new Result<Options>(programArgs);
        }

        private static void PrintHelp()
        {
            Console.WriteLine(Messages.Info_HelpUsage);
        }

        private static Result<Stream> CreateSourceFileStream(string path)
        {
            Stream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (ArgumentException)
            {
                return new Result<Stream>(Messages.Error_InvalidInputFilePath);
            }
            catch (NotSupportedException)
            {
                return new Result<Stream>(Messages.Error_InvalidInputFilePath);
            }
            catch (FileNotFoundException)
            {
                return new Result<Stream>(Messages.Error_InputFileIsNotFound);
            }
            catch (SecurityException)
            {
                return new Result<Stream>(Messages.Error_NoPermissionToAccessInputFile);
            }
            catch (UnauthorizedAccessException)
            {
                return new Result<Stream>(Messages.Error_NoAccessToInputFile);
            }

            return new Result<Stream>(stream);
        }

        private static Result<Stream> CreateDestFileStream(string path)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (ArgumentException)
            {
                return new Result<Stream>(Messages.Error_InvalidOutputFilePath);
            }
            catch (NotSupportedException)
            {
                return new Result<Stream>(Messages.Error_InvalidOutputFilePath);
            }
            catch (SecurityException)
            {
                return new Result<Stream>(Messages.Error_NoPermissionToAccessOutputFile);
            }
            catch (UnauthorizedAccessException)
            {
                return new Result<Stream>(Messages.Error_NoAccessToOutputPath);
            }

            return new Result<Stream>(stream);

        }
    }
}