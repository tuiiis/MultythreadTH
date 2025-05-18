using Asynchrony.Models;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Asynchrony.Classes
{
    public class DictionaryManager
    {
        private const string MergedFile = "merged.xml";
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
                        new XElement(nameof(Tank)+"s",
                            kvp.Value.Select(tank => new XElement(nameof(Tank),
                                new XElement(nameof(tank.ID), tank.ID),
                                new XElement(nameof(tank.Model), tank.Model),
                                new XElement(nameof(tank.SerialNumber), tank.SerialNumber),
                                new XElement(nameof(tank.TankType), tank.TankType.ToString()),
                                new XElement(nameof(tank.Manufacturer), tank.Manufacturer)
                            ))
                        )
                    );

                    xDocument.Save(kvp.Key);
                }));
            }

            await Task.WhenAll(tasks);
        }

        public static async Task MergeXmlFilesAsync(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            var mergedXDocument = new XDocument(new XElement(nameof(Tank) + "s"));

            await Task.Run(() =>
            {
                foreach (var kvp in dictionary)
                {
                    var xDocument = XDocument.Load(kvp.Key);
                    mergedXDocument.Root?.Add(xDocument.Root?.Elements());
                }
            });

            await Task.Run(() => mergedXDocument.Save(MergedFile));
        }
    }
}
