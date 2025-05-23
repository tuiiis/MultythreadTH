using TPL.Classes;
using TPL.Models;

namespace TPL;

class Program
{
    private static List<Tank>? tanks;
    private static List<Manufacturer>? manufacturers;

    static async Task Main(string[] args)
    {
        string fileNameTanks = Constants.FileNameTanks;
        string fileNameManufacturers = Constants.FileNameManufacturers;
        string fileNameMerged = Constants.FileNameMerged;

        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Create instances");
            Console.WriteLine("2. Serialize tanks in two threads");
            Console.WriteLine("3. Merge files");
            Console.WriteLine("4. Read merged file sequentially");
            Console.WriteLine("5. Read merged file in two threads");
            Console.WriteLine("6. Read merged file in ten threads");
            Console.WriteLine("Q. Quit");

            Console.Write("Enter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    (tanks, manufacturers) = ClassFaker.CreateInstances();
                    break;
                case "2":
                    try
                    {
                        ParallelSerializer.SerializeInTwoThreads(tanks!, manufacturers!, Constants.FileNameTanks, Constants.FileNameManufacturers);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Serialization error: {ex.Message} ({ex.GetType().Name})");
                    }
                    break;
                case "3":
                    await MergeManager.MergeFilesAsync(Constants.FileNameTanks, Constants.FileNameManufacturers, Constants.FileNameMerged);
                    break;
                case "4":
                    try
                    {
                        string content = ParallelReader.ReadSequentially(Constants.FileNameMerged);
                        Console.WriteLine("Content:\n" + content);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "5":
                    try
                    {
                        string content = ParallelReader.ReadInTwoThreads(Constants.FileNameMerged);
                        Console.WriteLine("Content:\n" + content);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "6":
                    try
                    {
                        string content = ParallelReader.ReadInTenThreads(Constants.FileNameMerged);
                        Console.WriteLine("Content:\n" + content);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "Q":
                case "q":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
