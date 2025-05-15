namespace TPLProject.Classes;

/// <summary>
/// Provides functionality for merging files.
/// </summary>
public static class MergeManager
{
    /// <summary>
    /// Merges two files containing tank and manufacturer data into a single merged file.
    /// </summary>
    /// <param name="tanksFile">The path to the file containing tank data.</param>
    /// <param name="manufacturersFile">The path to the file containing manufacturer data.</param>
    /// <param name="mergedFile">The path to the output file that will contain the merged data.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static async Task MergeFilesAsync(string tanksFile, string manufacturersFile, string mergedFile)
    {
        try
        {
            var reader = new ParallelReader(tanksFile, manufacturersFile, mergedFile);
            await reader.StartProcessingAsync();
            Console.WriteLine("Files merged successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error merging files: {ex.Message}");
        }
    }
}
