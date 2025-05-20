using System.Collections.Concurrent;
using Asynchrony.Models;

namespace Asynchrony.Classes
{
    public class DisplayHelper
    {
        public static void DisplayTanks(List<Tank> tanks)
        {
            foreach (var tank in tanks)
            {
                Console.WriteLine($"{nameof(Tank)} {nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}");
                Console.WriteLine($"{nameof(tank.Manufacturer)}: {tank.Manufacturer.Name}, {nameof(tank.Manufacturer.Address)}: {tank.Manufacturer.Address}, {nameof(tank.Manufacturer.IsAChildCompany)}: {tank.Manufacturer.IsAChildCompany}");
                Console.WriteLine();
            }
        }

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
