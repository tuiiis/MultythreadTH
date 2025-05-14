using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

namespace TPLProject.Classes;

public class ParallelReader
{
    private readonly string _file1;
    private readonly string _file2;
    private readonly string _outputFile;

    public ParallelReader(string file1, string file2, string outputFile)
    {
        _file1 = file1;
        _file2 = file2;
        _outputFile = outputFile;
    }

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

    private static string ReadFilePart(string filePath, long start, long size)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        fs.Seek(start, SeekOrigin.Begin);
        byte[] buffer = new byte[size];
        fs.Read(buffer, 0, (int)size);
        return Encoding.UTF8.GetString(buffer);
    }
}
