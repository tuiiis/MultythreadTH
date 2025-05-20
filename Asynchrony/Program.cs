using System.Collections.Concurrent;
using Asynchrony.Classes;
using Asynchrony.Models;
using System.Xml.Linq;

namespace Asynchrony;

class Program
{
    private static List<Tank>? tanks;
    private static ConcurrentDictionary<string, ConcurrentBag<Tank>>? dictionary;
    private static readonly XMLManager _xmlManager = new();

    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate 50 random Tank objects");
            Console.WriteLine("2. Save the tanks into 5 XML files");
            Console.WriteLine("3. Read XML files and fill a dictionary");
            Console.WriteLine("4. Merge dictionary into a single file");
            Console.WriteLine($"5. Turn {(_xmlManager.IsSortingEnabled ? "off" : "on")} sorting tanks every 5 seconds");
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
                        XMLManager.SaveGroupsToXML(tanks, 5);
                        Console.WriteLine($"\nTanks have been divided into 5 groups and saved to {nameof(FileConstants.TankFilePattern)} files.");
                    }
                    else
                    {
                        Console.WriteLine("\nPlease generate tanks first!");
                    }
                    break;
                case "3":
                    try
                    {
                        dictionary = await ClassManager.ProcessXmlFilesAsync(tanks!);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"\n{ex.Message}");
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine($"\n{ex.Message}");
                    }
                    break;
                case "4":
                    try
                    {
                        await ClassManager.MergeTanksToFileAsync(dictionary!);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"\n{ex.Message}");
                    }
                    break;
                case "5":
                    if (dictionary == null)
                    {
                        Console.WriteLine("\nPlease read XML files first!");
                        break;
                    }

                    if (_xmlManager.IsSortingEnabled)
                    {
                        _xmlManager.StopSorting();
                        Console.WriteLine("\nTank sorting has been disabled.");
                    }
                    else
                    {
                        _xmlManager.StartSorting(dictionary);
                        Console.WriteLine("\nTank sorting has been enabled.");
                    }
                    break;
                case "Q":
                case "q":
                    _xmlManager.StopSorting();
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
