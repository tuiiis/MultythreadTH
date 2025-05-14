using TPLProject.Models;
using System.Xml.Linq;

namespace TPLProject.Classes;

public class ParallelSerializer
{
    public static void SerializeTanksPart(List<Tank> tanks, string filename)
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
            throw new Exception($"Error in {nameof(SerializeTanksPart)}: {ex.Message}", ex);
        }
    }

    public static void SerializeManufacturersPart(List<Manufacturer> manufacturers, string filename)
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

    public static void SerializeInTwoThreads(List<Tank> tanks, List<Manufacturer> manufacturers, string tanksFile, string manufacturersFile)
    {
        if (tanks == null || tanks.Count < 10 || manufacturers == null || manufacturers.Count < 10)
        {
            throw new InvalidOperationException("Instances must be created first.");
        }

        Task task1 = Task.Run(() => SerializeTanksPart(tanks, tanksFile));
        Task task2 = Task.Run(() => SerializeManufacturersPart(manufacturers, manufacturersFile));

        Task.WaitAll(task1, task2);
        Console.WriteLine($"Serialization completed. Files saved to: {tanksFile} and {manufacturersFile}");
    }
}
