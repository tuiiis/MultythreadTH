using System.Collections.Concurrent;
using Asynchrony.Classes;
using Asynchrony.Models;

namespace Asynchrony;

class Program
{
    private static List<Tank>? tanks;
    private static ConcurrentDictionary<string, ConcurrentBag<Tank>>? dictionary;
    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate 50 random Tank objects");
            Console.WriteLine("2. Divide the tanks into 5 groups and save to XML files");
            Console.WriteLine("3. Merge XML files and display contents");
            Console.WriteLine("Q. Quit");

            Console.Write("Enter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    tanks = ClassFaker.CreateTanks(50);
                    DisplayHelper.DisplayTanks(tanks);
                    break;
                case "2":
                    if (tanks != null)
                    {
                        dictionary = DictionaryManager.SplitByFive(tanks);
                        await DictionaryManager.SaveToXmlAsync(dictionary);
                        Console.WriteLine("\nTanks have been divided into 5 groups and saved to XML files.");
                    }
                    else
                    {
                        Console.WriteLine("\nPlease generate tanks first!");
                    }
                    break;
                case "3":
                    if (tanks != null && dictionary != null)
                    {
                        await DictionaryManager.MergeXmlFilesAsync(dictionary);
                        DisplayHelper.OutputDictionaryContents(dictionary);
                    }
                    else
                    {
                        Console.WriteLine("\nPlease generate tanks and divide them into groups first!");
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
