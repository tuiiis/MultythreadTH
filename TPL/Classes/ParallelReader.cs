using System.Text;
using System.Diagnostics;

namespace TPLProject.Classes;

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

    public ParallelReader(string file1, string file2, string outputFile)
    {
        _file1 = file1;
        _file2 = file2;
        _outputFile = outputFile;
        _writer = new StreamWriter(outputFile, true); // append mode
    }

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
                        System.Threading.Monitor.Wait(_lock);
                    }

                    _writer.WriteLine(line);
                    Console.WriteLine($"File1 wrote: {line}");
                    _file1Turn = false;
                    _writer.Flush();
                    System.Threading.Monitor.PulseAll(_lock);
                }
            }

            lock (_lock)
            {
                _file1Done = true;
                System.Threading.Monitor.PulseAll(_lock);
            }
        }
    }

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
                        System.Threading.Monitor.Wait(_lock);
                    }

                    _writer.WriteLine(line);
                    Console.WriteLine($"File2 wrote: {line}");
                    _file1Turn = true;
                    _writer.Flush();
                    System.Threading.Monitor.PulseAll(_lock);
                }
            }

            lock (_lock)
            {
                _file2Done = true;
                System.Threading.Monitor.PulseAll(_lock);
            }
        }
    }

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

        Console.WriteLine($"[10 Threads (5 at a time)] Read time: {stopwatch.ElapsedTicks * (1_000_000_000.0 / Stopwatch.Frequency)} ns");
        return string.Concat(tasks.Select(t => t.Result));
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
