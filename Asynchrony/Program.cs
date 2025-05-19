using System.Collections.Concurrent;
using Asynchrony.Classes;
using Asynchrony.Models;
using System.Xml.Linq;

namespace Asynchrony;

class Program
{
    private static List<Tank>? tanks;
    private static ConcurrentDictionary<string, ConcurrentBag<Tank>>? dictionary;
    private static bool isSortingEnabled = false;
    private static TankSorter? tankSorter;

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
                    try
                    {
                        dictionary = await TankManager.ProcessXmlFilesAsync(tanks!);
                        tankSorter = new TankSorter(dictionary);
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
                        await TankManager.MergeTanksToFileAsync(dictionary!);
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

                    isSortingEnabled = !isSortingEnabled;
                    if (isSortingEnabled)
                    {
                        tankSorter?.StartSorting();
                        Console.WriteLine("\nTank sorting has been enabled.");
                    }
                    else
                    {
                        tankSorter?.StopSorting();
                        Console.WriteLine("\nTank sorting has been disabled.");
                    }
                    break;
                case "Q":
                case "q":
                    tankSorter?.StopSorting();
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task<List<Tank>> ReadXmlFileAsync(string filePath, IProgress<int> progress)
    {
        var tanks = new List<Tank>();
        var doc = XDocument.Load(filePath);
        var tankElements = doc.Descendants("Tank").ToList();
        int totalTanks = tankElements.Count;
        int processedTanks = 0;

        foreach (var tankElement in tankElements)
        {
            var tank = new Tank
            {
                ID = int.Parse(tankElement.Element("ID")?.Value ?? "0"),
                Model = tankElement.Element("Model")?.Value ?? string.Empty,
                SerialNumber = tankElement.Element("SerialNumber")?.Value ?? string.Empty,
                TankType = (TankType)Enum.Parse(typeof(TankType), tankElement.Element("TankType")?.Value ?? "Light")
            };

            tanks.Add(tank);
            processedTanks++;
            progress.Report((int)((double)processedTanks / totalTanks * 100));
            await Task.Delay(100); // Slow down for demo purposes
        }

        return tanks;
    }
}
