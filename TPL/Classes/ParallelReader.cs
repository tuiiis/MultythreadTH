using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

namespace TPL.Classes;

/// <summary>
/// Provides functionality for reading files in parallel and writing to an output file.
/// </summary>
public class ParallelReader
{
    private readonly string _file1;
    private readonly string _file2;
    private readonly string _outputFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelReader"/> class.
    /// </summary>
    /// <param name="file1">The path to the first input file.</param>
    /// <param name="file2">The path to the second input file.</param>
    /// <param name="outputFile">The path to the output file.</param>
    public ParallelReader(string file1, string file2, string outputFile)
    {
        _file1 = file1;
        _file2 = file2;
        _outputFile = outputFile;
    }

    /// <summary>
    /// Starts the parallel processing of the input files and writing to the output file.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task StartProcessingAsync()
    {
        var queue = new ConcurrentQueue<string>();
        var cts = new CancellationTokenSource();
        var tasks = new Task[]
        {
            Task.Run(async () => await ReadFileAsync(_file1, queue, cts)),
            Task.Run(async () => await ReadFileAsync(_file2, queue, cts)),
            Task.Run(async () => await WriteToFileAsync(_outputFile, queue, cts))
        };

        await Task.WhenAll(tasks);
    }

    private int _readingTasksCompleted = 0;
    private readonly object _lock = new object();

    private void IncrementReadingTasksCompleted()
    {
        lock (_lock)
        {
            _readingTasksCompleted++;
        }
    }

    private bool IsReadingCompleted()
    {
        lock (_lock)
        {
            return _readingTasksCompleted >= 2;
        }
    }

    /// <summary>
    /// Reads a file asynchronously and enqueues its lines to a concurrent queue.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="queue">The concurrent queue to enqueue the lines to.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task ReadFileAsync(string filePath, ConcurrentQueue<string> queue, CancellationTokenSource cts)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null && !cts.IsCancellationRequested)
            {
                queue.Enqueue(line);
                Console.WriteLine($"{filePath} read: {line}");
                await Task.Yield(); // Allow other tasks to run
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Reading from {filePath} was cancelled.");
        }
        finally
        {
            IncrementReadingTasksCompleted();
        }
    }

    /// <summary>
    /// Writes lines from a concurrent queue to a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to write to.</param>
    /// <param name="queue">The concurrent queue to dequeue lines from.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task WriteToFileAsync(string filePath, ConcurrentQueue<string> queue, CancellationTokenSource cts)
    {
        using var writer = new StreamWriter(filePath, true); // append mode
        while (!cts.IsCancellationRequested)
        {
            if (queue.TryDequeue(out var line))
            {
                await writer.WriteLineAsync(line);
                Console.WriteLine($"Wrote: {line}");
                await writer.FlushAsync();
            }
            else
            {
                // Check if both reading tasks are completed
                if (IsReadingCompleted())
                {
                    cts.Cancel();
                    break;
                }
                await Task.Delay(10, cts.Token); // Wait before checking again
            }
        }
    }

    /// <summary>
    /// Reads the contents of a file sequentially.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadSequentially(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Merged file not found. Please merge files first.", filePath);

        var stopwatch = Stopwatch.StartNew();
        string content = File.ReadAllText(filePath);
        stopwatch.Stop();
        Console.WriteLine($"[Sequential] Read time: {stopwatch.ElapsedTicks * (1_000_000_000.0 / Stopwatch.Frequency)} ns");
        return content;
    }

    /// <summary>
    /// Reads the contents of a file using two threads.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
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
        Console.WriteLine($"[Two Threads] Read time: {stopwatch.ElapsedTicks * (1_000_000_000.0 / Stopwatch.Frequency)} ns");
        return part1 + part2;
    }

    /// <summary>
    /// Reads the contents of a file using multiple threads.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadInTenThreads(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Merged file not found. Please merge files first.", filePath);

        const int threadCount = 10;
        const int maxDegreeOfParallelism = 5;

        var stopwatch = Stopwatch.StartNew();
        long length = new FileInfo(filePath).Length;
        long chunkSize = length / threadCount;

        var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        var parts = new string[threadCount];

        Parallel.For(0, threadCount, options, index =>
        {
            long start = index * chunkSize;
            long size = (index == threadCount - 1) ? (length - start) : chunkSize;
            parts[index] = ReadFilePart(filePath, start, size);
        });

        stopwatch.Stop();

        Console.WriteLine($"[10 Threads ({maxDegreeOfParallelism} at a time)] Read time: {stopwatch.ElapsedTicks * (1_000_000_000.0 / Stopwatch.Frequency)} ns");
        return string.Concat(parts);
    }

    /// <summary>
    /// Reads a part of a file.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="start">The starting position in the file.</param>
    /// <param name="size">The number of bytes to read.</param>
    /// <returns>The contents of the file part as a string.</returns>
    private static string ReadFilePart(string filePath, long start, long size)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        fs.Seek(start, SeekOrigin.Begin);
        byte[] buffer = new byte[size];
        fs.Read(buffer, 0, (int)size);
        return Encoding.UTF8.GetString(buffer);
    }
}
