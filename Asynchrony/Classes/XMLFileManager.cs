using System.Xml.Linq;
using Asynchrony.Models;
using System.Collections.Concurrent;

namespace Asynchrony.Classes
{
    public static class XMLFileManager
    {
        public static void SaveToXML(string filePath, IEnumerable<Tank> tanks)
        {
            var xDoc = new XDocument(
                new XElement("Tanks",
                    tanks.Select(t =>
                        new XElement("Tank",
                            new XElement("ID", t.ID),
                            new XElement("Model", t.Model),
                            new XElement("SerialNumber", t.SerialNumber),
                            new XElement("TankType", t.TankType),
                            new XElement("Manufacturer", t.Manufacturer)
                        )
                    )
                )
            );

            xDoc.Save(filePath);
        }

        public static async Task<List<Tank>> ReadFromXMLAsync(string filePath, IProgress<int> progress)
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
} 