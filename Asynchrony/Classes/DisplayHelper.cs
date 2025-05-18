using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Asynchrony.Models;

namespace Asynchrony.Classes
{
    public class DisplayHelper
    {
        public static void DisplayTanks(List<Tank> tanks)
        {
            foreach (var tank in tanks)
            {
                Console.WriteLine($"Tank ID: {tank.ID}, Model: {tank.Model}, Serial Number: {tank.SerialNumber}, Type: {tank.TankType}");
                Console.WriteLine($"Manufacturer: {tank.Manufacturer.Name}, Address: {tank.Manufacturer.Address}, Is Child Company: {tank.Manufacturer.IsAChildCompany}");
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
                    Console.WriteLine($"ID: {tank.ID}, Model: {tank.Model}, SerialNumber: {tank.SerialNumber}, TankType: {tank.TankType}, Manufacturer: {tank.Manufacturer}");
                }
                Console.WriteLine();
            }
        }
    }
}
