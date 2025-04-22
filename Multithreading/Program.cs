﻿using Multithreading.Models;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;
using Multithreading.Classes;

namespace Multithreading
{
    class Program
    {
        private const string FileNameMerged = "merged.txt";
        private const string FileNameTanks = "tanks.xml";
        private const string FileNameManufacturers = "manufacturers.xml";
        static List<Tank>? tanks;
        static List<Manufacturer>? manufacturers;

        static void Main()
        {
            while (true)
            {
                Console.WriteLine("1. Create 20 instances");
                Console.WriteLine("2. Serialize tanks in two threads");
                Console.WriteLine("3. Merge files in parallel");
                Console.WriteLine("4. Read merged files sequentially");
                Console.WriteLine("5. Read merged files file in two threads");
                Console.WriteLine("6. Read merged files with 10 threads (5 at a time)");
                Console.WriteLine("7. Exit");
                Console.Write("Choose an option: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateInstances();
                        break;
                    case "2":
                        SerializeInTwoThreads();
                        break;
                    case "3":
                        MergeFiles();
                        break;
                    case "4":
                        ReadMergedFileSequentially();
                        break;
                    case "5":
                        ReadMergedFileTwoThreads();
                        break;
                    case "6":
                        ReadMergedFileTenThreads();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please choose 1-7.");
                        break;
                }
            }
        }

        private static void CreateInstances()
        {
            tanks = ClassFaker.TankFaker.Generate(10);
            manufacturers = ClassFaker.ManufacturerFaker.Generate(10);
            Console.WriteLine("10 tanks and 10 manufacturers created:");

            Console.WriteLine("Tanks:");
            foreach (var tank in tanks)
            {
                Console.WriteLine($"{nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}");
            }

            Console.WriteLine("\nManufacturers:");
            foreach (var m in manufacturers)
            {
                Console.WriteLine($"{nameof(m.Name)}: {m.Name}, {nameof(m.Address)}: {m.Address}, {nameof(m.IsAChildCompany)}: {m.IsAChildCompany}");
            }
        }

        // Tanks serialization/merge methods
        private static void MergeFiles()
        {
            try
            {
                var reader = new ParallelReader(FileNameTanks, FileNameManufacturers, FileNameMerged);
                reader.StartProcessing();
                Console.WriteLine("Files merged successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error merging files: {ex.Message}");
            }
        }

        private static void SerializeInTwoThreads()
        {
            if (tanks == null || tanks.Count < 10 || manufacturers == null || manufacturers.Count < 10)
            {
                Console.WriteLine("Error: First create the instances (option 1).");
                return;
            }

            try
            {
                ParallelSerializer serializer = new ParallelSerializer();

                Task task1 = Task.Run(() => serializer.SerializeManufacturersPart(manufacturers, FileNameManufacturers));
                Task task2 = Task.Run(() => serializer.SerializeTankPart(tanks, FileNameTanks));

                Task.WaitAll(task1, task2);
                Console.WriteLine($"Serialization completed. Files saved to: {FileNameTanks} and {FileNameManufacturers}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serialization error: {ex.Message} ({ex.GetType().Name})");
            }
        }

        // Reading methods for tanks
        private static void ReadMergedFileSequentially()
        {
            if (!File.Exists(FileNameMerged))
            {
                Console.WriteLine("Merged file not found. Please merge files first (option 3).");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            string content = File.ReadAllText(FileNameMerged);
            stopwatch.Stop();
            Console.WriteLine("Content:\n" + content);
            Console.WriteLine($"[Sequential] Read time: {stopwatch.ElapsedMilliseconds} ms");
        }


        private static void ReadMergedFileTwoThreads()
        {
            if (!File.Exists(FileNameMerged))
            {
                Console.WriteLine("Merged file not found. Please merge files first (option 3).");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            string part1, part2;

            long length = new FileInfo(FileNameMerged).Length;
            long mid = length / 2;

            Task<string> task1 = Task.Run(() =>
            {
                using (var fs = new FileStream(FileNameMerged, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[mid];
                    fs.Read(buffer, 0, (int)mid);
                    return Encoding.UTF8.GetString(buffer);
                }
            });

            Task<string> task2 = Task.Run(() =>
            {
                using (var fs = new FileStream(FileNameMerged, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(mid, SeekOrigin.Begin);
                    byte[] buffer = new byte[length - mid];
                    fs.Read(buffer, 0, (int)(length - mid));
                    return Encoding.UTF8.GetString(buffer);
                }
            });

            Task.WaitAll(task1, task2);
            stopwatch.Stop();

            part1 = task1.Result;
            part2 = task2.Result;
            string fullContent = part1 + part2;

            Console.WriteLine("Content:\n" + fullContent);
            Console.WriteLine($"[Two Threads] Read time: {stopwatch.ElapsedMilliseconds} ms");
        }


        private static void ReadMergedFileTenThreads()
        {
            if (!File.Exists(FileNameMerged))
            {
                Console.WriteLine("Merged file not found. Please merge files first (option 3).");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            const int threadCount = 10;
            const int concurrencyLimit = 5;
            var semaphore = new SemaphoreSlim(concurrencyLimit, concurrencyLimit);

            var tasks = new List<Task>();
            var parts = new string[threadCount];

            long length = new FileInfo(FileNameMerged).Length;
            long chunkSize = length / threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        using (var fs = new FileStream(FileNameMerged, FileMode.Open, FileAccess.Read))
                        {
                            long start = index * chunkSize;
                            long end = (index == threadCount - 1) ? length : (index + 1) * chunkSize;
                            long size = end - start;

                            fs.Seek(start, SeekOrigin.Begin);
                            byte[] buffer = new byte[size];
                            fs.Read(buffer, 0, (int)size);
                            parts[index] = Encoding.UTF8.GetString(buffer);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            string fullContent = string.Concat(parts);
            Console.WriteLine("Content:\n" + fullContent);
            Console.WriteLine($"[10 Threads (5 at a time)] Read time: {stopwatch.ElapsedMilliseconds} ms");
        }

        public class ParallelSerializer
        {
            public void SerializeTankPart(List<Tank> tanks, string filename)
            {
                try
                {
                    var tanksDoc = new XDocument(
                        new XElement("Tanks",
                            from tank in tanks
                            select new XElement("Tank",
                                new XElement(nameof(tank.ID), tank.ID),
                                new XElement(nameof(tank.Model), tank.Model),
                                new XElement(nameof(tank.SerialNumber), tank.SerialNumber),
                                new XElement(nameof(tank.TankType), tank.TankType)
                            )
                        )
                    );
                    tanksDoc.Save(filename);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in {nameof(SerializeTankPart)}: {ex.Message}", ex);
                }
            }

            public void SerializeManufacturersPart(List<Manufacturer> manufacturers, string filename)
            {
                try
                {
                    var manufacturersDoc = new XDocument(
                        new XElement("Manufacturers",
                            from m in manufacturers
                            select new XElement("Manufacturer",
                                new XElement(nameof(m.Name), m.Name),
                                new XElement(nameof(m.Address), m.Address),
                                new XElement(nameof(m.IsAChildCompany), m.IsAChildCompany)
                            )
                        )
                    );
                    manufacturersDoc.Save(filename);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in {nameof(SerializeManufacturersPart)}: {ex.Message}", ex);
                }
            }

        }
    }
}
