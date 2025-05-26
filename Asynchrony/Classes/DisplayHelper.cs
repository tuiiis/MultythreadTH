using System.Collections.Concurrent;
using Asynchrony.Models;

namespace Asynchrony.Classes
{
    /// <summary>
    /// A utility class for displaying tank data.
    /// </summary>
    public class DisplayHelper
    {
        /// <summary>
        /// Displays the details of a list of tanks.
        /// </summary>
        /// <param name="tanks">The list of tanks to display.</param>
        public static void DisplayTanks(List<Tank> tanks)
        {
            foreach (var tank in tanks)
            {
                Console.WriteLine($"{nameof(Tank)} {nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}");
                Console.WriteLine($"{nameof(tank.Manufacturer)}: {tank.Manufacturer.Name}, {nameof(tank.Manufacturer.Address)}: {tank.Manufacturer.Address}, {nameof(tank.Manufacturer.IsAChildCompany)}: {tank.Manufacturer.IsAChildCompany}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Outputs the contents of a dictionary containing tank data.
        /// </summary>
        /// <param name="dictionary">The dictionary to output.</param>
        public static void OutputDictionaryContents(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                Console.WriteLine($"Group: {kvp.Key}");
                foreach (var tank in kvp.Value)
                {
                    Console.WriteLine($"{nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}, {nameof(tank.Manufacturer)}: {tank.Manufacturer}");
                }
                Console.WriteLine();
            }
        }
    }
}
