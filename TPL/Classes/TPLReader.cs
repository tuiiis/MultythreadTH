using System.Text;

namespace TPLProject.Classes;

public class TPLReader
{
    public static string ReadSequentially(string mergedFile)
    {
        try
        {
            using (var reader = new StreamReader(mergedFile))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading file sequentially: {ex.Message}", ex);
        }
    }

    public static string ReadInTwoThreads(string mergedFile)
    {
        try
        {
            var tasks = new[]
            {
                Task.Run(() => ReadFilePart(mergedFile, 0, new FileInfo(mergedFile).Length / 2)),
                Task.Run(() => ReadFilePart(mergedFile, new FileInfo(mergedFile).Length / 2, new FileInfo(mergedFile).Length))
            };

            Task.WaitAll(tasks);
            return string.Concat(tasks.Select(t => t.Result));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading file in two threads: {ex.Message}", ex);
        }
    }

    public static string ReadInTenThreads(string mergedFile)
    {
        try
        {
            var fileInfo = new FileInfo(mergedFile);
            var chunkSize = fileInfo.Length / 10;
            var tasks = new Task<string>[10];

            for (int i = 0; i < 10; i++)
            {
                long start = i * chunkSize;
                long end = (i == 9) ? fileInfo.Length : (i + 1) * chunkSize;
                tasks[i] = Task.Run(() => ReadFilePart(mergedFile, start, end));
            }

            Task.WaitAll(tasks);
            return string.Concat(tasks.Select(t => t.Result));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading file in ten threads: {ex.Message}", ex);
        }
    }

    private static string ReadFilePart(string filePath, long start, long end)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.Position = start;
            var length = end - start;
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
