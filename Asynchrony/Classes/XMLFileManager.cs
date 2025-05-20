using System.Xml.Linq;
using Asynchrony.Models;

namespace Asynchrony.Classes
{
    public static class XMLFileManager
    {
        public static void SaveToXML(string filePath, IEnumerable<Tank> tanks)
        {
            var xDoc = new XDocument(
                new XElement(nameof(Tank)+"s",
                    tanks.Select(t =>
                        new XElement(nameof(Tank),
                            new XElement(nameof(Tank.ID), t.ID),
                            new XElement(nameof(Tank.Model), t.Model),
                            new XElement(nameof(Tank.SerialNumber), t.SerialNumber),
                            new XElement(nameof(Tank.TankType), t.TankType),
                            new XElement(nameof(Tank.Manufacturer), t.Manufacturer)
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
            var tankElements = doc.Descendants(nameof(Tank)).ToList();
            int totalTanks = tankElements.Count;
            int processedTanks = 0;

            foreach (var tankElement in tankElements)
            {
                var tank = new Tank
                {
                    ID = int.Parse(tankElement.Element(nameof(Tank.ID))?.Value ?? "0"),
                    Model = tankElement.Element(nameof(Tank.Model))?.Value ?? string.Empty,
                    SerialNumber = tankElement.Element(nameof(Tank.SerialNumber))?.Value ?? string.Empty,
                    TankType = (TankType)Enum.Parse(typeof(TankType), tankElement.Element(nameof(Tank.TankType))?.Value ?? nameof(TankType.Light))
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