using System.Text;

namespace TPL.Classes;

/// <summary>
/// Provides functionality for reading files using different reading strategies.
/// </summary>
public class TPLReader
{
    /// <summary>
    /// Reads the contents of a file sequentially.
    /// </summary>
    /// <param name="mergedFile">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
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

    /// <summary>
    /// Reads the contents of a file using two threads.
    /// </summary>
    /// <param name="mergedFile">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
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

    /// <summary>
    /// Reads the contents of a file using ten threads.
    /// </summary>
    /// <param name="mergedFile">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
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

    /// <summary>
    /// Reads a part of a file.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="start">The starting position in the file.</param>
    /// <param name="end">The ending position in the file.</param>
    /// <returns>The contents of the file part as a string.</returns>
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
