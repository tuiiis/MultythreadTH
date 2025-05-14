namespace TPLProject.Classes;

public static class MergeManager
{
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
