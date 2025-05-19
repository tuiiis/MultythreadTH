using System.Collections.Concurrent;
using Asynchrony.Classes;
using Asynchrony.Models;

namespace Asynchrony;

class Program
{
    private static List<Tank>? tanks;
    private static ConcurrentDictionary<string, ConcurrentBag<Tank>>? dictionary;
    private static bool isSortingEnabled = false;
    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate 50 random Tank objects");
            Console.WriteLine("2. Save the tanks into 5 XML files");
            Console.WriteLine("3. Read XML files and fill a dictionary");
            Console.WriteLine("4. Merge dictionary into a single file");
            Console.WriteLine($"5. Turn {(isSortingEnabled ? "off" : "on")} sorting tanks every 5 seconds");
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
                        var xmlManager = new XMLManager();
                        xmlManager.SaveGroupsToXML(tanks, 5);
                        Console.WriteLine("\nTanks have been divided into 5 groups and saved to XML files.");
                    }
                    else
                    {
                        Console.WriteLine("\nPlease generate tanks first!");
                    }
                    break;
                case "3":
                    // condition: tanks != null and files exist, otherwise display a message
                    // read XML files WITH 5 THREADS (add progress bar when reading, for demo slow down after reading one object for 100ms) and put them into a single dictionary (key = file name, value = list of tanks)
                    // display the contents of the dictionary
                    break;
                case "4":
                    //read concurrent dictionary in 5 threads and fill one file with all tanks
                    // display the contents of the file
                    break;
                case "5":
                    // turn on/off the sorting of the tanks every 5 seconds
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
