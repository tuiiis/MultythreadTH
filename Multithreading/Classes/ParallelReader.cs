using System.Text;
using System.Diagnostics;

namespace Multithreading.Classes;

/// <summary>
/// Provides functionality for reading and merging two files in parallel, as well as reading a merged file using different threading approaches.
/// </summary>
public class ParallelReader
{
    private readonly string _file1;
    private readonly string _file2;
    private readonly string _outputFile;
    private readonly object _lock = new object();
    private bool _file1Turn = true;
    private bool _file1Done = false;
    private bool _file2Done = false;
    private StreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelReader"/> class.
    /// </summary>
    /// <param name="file1">Path to the first input file.</param>
    /// <param name="file2">Path to the second input file.</param>
    /// <param name="outputFile">Path to the output file where merged content will be written.</param>
    public ParallelReader(string file1, string file2, string outputFile)
    {
        _file1 = file1;
        _file2 = file2;
        _outputFile = outputFile;
        _writer = new StreamWriter(outputFile, true); // append mode
    }

    /// <summary>
    /// Processes the first input file, reading it line by line and writing to the output file in an alternating manner with the second file.
    /// </summary>
    private void ProcessFile1()
    {
        using (var reader = new StreamReader(_file1))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lock (_lock)
                {
                    while (!_file1Turn && !_file2Done)
                    {
                        Monitor.Wait(_lock);
                    }

                    _writer.WriteLine(line);
                    Console.WriteLine($"File1 wrote: {line}");
                    _file1Turn = false;
                    _writer.Flush();
                    Monitor.PulseAll(_lock);
                }
            }

            lock (_lock)
            {
                _file1Done = true;
                Monitor.PulseAll(_lock);
            }
        }
    }

    /// <summary>
    /// Processes the second input file, reading it line by line and writing to the output file in an alternating manner with the first file.
    /// </summary>
    private void ProcessFile2()
    {
        using (var reader = new StreamReader(_file2))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lock (_lock)
                {
                    while (_file1Turn && !_file1Done)
                    {
                        Monitor.Wait(_lock);
                    }

                    _writer.WriteLine(line);
                    Console.WriteLine($"File2 wrote: {line}");
                    _file1Turn = true;
                    _writer.Flush();
                    Monitor.PulseAll(_lock);
                }
            }

            lock (_lock)
            {
                _file2Done = true;
                Monitor.PulseAll(_lock);
            }
        }
    }

    /// <summary>
    /// Starts the parallel processing of the two input files.
    /// </summary>
    public void StartProcessing()
    {
        Thread t1 = new Thread(ProcessFile1);
        Thread t2 = new Thread(ProcessFile2);
        t1.Start();
        t2.Start();
        t1.Join();
        t2.Join();
        _writer.Close();
    }

    /// <summary>
    /// Reads the contents of a file sequentially.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadSequentially(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Merged file not found. Please merge files first.", filePath);

        var stopwatch = Stopwatch.StartNew();
        string content = File.ReadAllText(filePath);
        stopwatch.Stop();
        Console.WriteLine($"[Sequential] Read time: {stopwatch.ElapsedMilliseconds} ms");
        return content;
    }

    /// <summary>
    /// Reads the contents of a file using two threads.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadInTwoThreads(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Merged file not found. Please merge files first.", filePath);

        var stopwatch = Stopwatch.StartNew();
        long length = new FileInfo(filePath).Length;
        long mid = length / 2;

        string part1 = ReadFilePart(filePath, 0, mid);
        string part2 = ReadFilePart(filePath, mid, length - mid);

        stopwatch.Stop();
        Console.WriteLine($"[Two Threads] Read time: {stopwatch.ElapsedMilliseconds} ms");
        return part1 + part2;
    }

    /// <summary>
    /// Reads the contents of a file using ten threads with a concurrency limit of five.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadInTenThreads(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Merged file not found. Please merge files first.", filePath);

        const int threadCount = 10;
        const int concurrencyLimit = 5;
        var semaphore = new SemaphoreSlim(concurrencyLimit, concurrencyLimit);

        var stopwatch = Stopwatch.StartNew();
        long length = new FileInfo(filePath).Length;
        long chunkSize = length / threadCount;

        var tasks = new Task<string>[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    long start = index * chunkSize;
                    long size = (index == threadCount - 1) ? (length - start) : chunkSize;
                    return ReadFilePart(filePath, start, size);
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"[10 Threads (5 at a time)] Read time: {stopwatch.ElapsedMilliseconds} ms");
        return string.Concat(tasks.Select(t => t.Result));
    }

    /// <summary>
    /// Reads a part of a file from a specified start position with a given size.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <param name="start">Start position in the file.</param>
    /// <param name="size">Size of the data to be read.</param>
    /// <returns>The contents of the specified file part as a string.</returns>
    private static string ReadFilePart(string filePath, long start, long size)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        fs.Seek(start, SeekOrigin.Begin);
        byte[] buffer = new byte[size];
        fs.Read(buffer, 0, (int)size);
        return Encoding.UTF8.GetString(buffer);
    }
}
