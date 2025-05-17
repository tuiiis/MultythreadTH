using Asynchrony.Models;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Asynchrony.Classes
{
    public class DictionaryManager
    {
        private static readonly string[] fileNames = new string[] { "TanksGroup1.xml", "TanksGroup2.xml", "TanksGroup3.xml", "TanksGroup4.xml", "TanksGroup5.xml" };

        public static ConcurrentDictionary<string, ConcurrentBag<Tank>> SplitByFive(List<Tank> tanks)
        {
            var tankGroups = tanks.Select((tank, index) => new { tank, index })
            .GroupBy(x => x.index / 10)
            .Select(g => g.Select(x => x.tank).ToList())
            .ToList();

            var concurrentDictionary = new ConcurrentDictionary<string, ConcurrentBag<Tank>>();

            for (int i = 0; i < tankGroups.Count; i++)
            {
                concurrentDictionary.TryAdd(fileNames[i], new ConcurrentBag<Tank>(tankGroups[i]));
            }

            return concurrentDictionary;
        }

        public static async Task SaveToXmlAsync(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            var tasks = new List<Task>();

            foreach (var kvp in dictionary)
            {
                tasks.Add(Task.Run(() =>
                {
                    var xDocument = new XDocument(
                        new XElement("Tanks",
                            kvp.Value.Select(tank => new XElement("Tank",
                                new XElement("ID", tank.ID),
                                new XElement("Model", tank.Model),
                                new XElement("SerialNumber", tank.SerialNumber),
                                new XElement("TankType", tank.TankType),
                                new XElement("Manufacturer", tank.Manufacturer)
                            ))
                        )
                    );

                    xDocument.Save(kvp.Key);
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
