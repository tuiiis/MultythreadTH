using System;
using System.IO;
using System.Threading;

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
}