namespace Multithreading.Classes;

/// <summary>
/// Provides a method for merging two files into a single output file.
/// </summary>
public static class MergeManager
{
    /// <summary>
    /// Merges the contents of two input files into a single output file.
    /// </summary>
    /// <param name="tanksFile">Path to the first input file.</param>
    /// <param name="manufacturersFile">Path to the second input file.</param>
    /// <param name="mergedFile">Path to the output file where merged content will be written.</param>
    public static void MergeFiles(string tanksFile, string manufacturersFile, string mergedFile)
    {
        try
        {
            var reader = new ParallelReader(tanksFile, manufacturersFile, mergedFile);
            reader.StartProcessing();
            Console.WriteLine("Files merged successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error merging files: {ex.Message}");
        }
    }
}
