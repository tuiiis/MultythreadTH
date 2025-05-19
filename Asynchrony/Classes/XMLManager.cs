using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Asynchrony.Models;

namespace Asynchrony.Classes
{
    public class XMLManager
    {
        public void SaveGroupsToXML(List<Tank> tanks, int numberOfGroups = 5)
        {
            var groups = Split(tanks, numberOfGroups);
            for (int i = 0; i < groups.Count; i++)
            {
                var xDoc = new XDocument(
                    new XElement("Tanks",
                        groups[i].Select(t =>
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

                xDoc.Save($"tanks_{i+1}.xml");
            }
        }

        public List<List<Tank>> Split(List<Tank> tanks, int numberOfGroups)
        {
            var groups = new List<List<Tank>>();
            int tanksPerGroup = tanks.Count / numberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                int start = i * tanksPerGroup;
                int count = (i == numberOfGroups - 1) ? tanks.Count - start : tanksPerGroup;
                groups.Add(tanks.Skip(start).Take(count).ToList());
            }
            return groups;
        }
    }
}
