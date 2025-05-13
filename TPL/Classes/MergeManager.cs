namespace TPLProject.Classes;

public static class MergeManager
{
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
