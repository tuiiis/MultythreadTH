using Asynchrony.Classes;
using Asynchrony.Models;

namespace Asynchrony;

class Program
{
    private static List<Tank>? tanks;
    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate 50 random Tank objects");
            Console.WriteLine("2. Divide the tanks into 5 groups");
            Console.WriteLine("3. Option 3");
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
                        var dictionary = DictionaryManager.SplitByFive(tanks);
                        await DictionaryManager.SaveToXmlAsync(dictionary);
                        Console.WriteLine("Tanks have been divided into 5 groups and saved to XML files.");
                    }
                    else
                    {
                        Console.WriteLine("Please generate tanks first.");
                    }
                    break;
                case "3":
                    // Implement Option 3 logic here
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
