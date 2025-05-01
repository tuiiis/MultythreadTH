using Serialization.Models;
using Serialization.Helpers;
using System.Collections.Generic;

List<Manufacturer>? manufacturers = null;
List<Tank>? tanks = null;

while (true)
{
    MenuHelper.ShowMenu();
    var choice = MenuHelper.ReadMenuChoice();

    switch (choice)
    {
        case "1":
            manufacturers = ClassFaker.ManufacturerFaker.Generate(10);
            tanks = ClassFaker.TankFaker.Generate(10);

            Console.WriteLine(Constants.ManufacturersCreated);
            foreach (var m in manufacturers)
            {
                Console.WriteLine($"Name: {m.Name}, Address: {m.Address}, IsAChildCompany: {m.IsAChildCompany}");
            }

            Console.WriteLine(Constants.TanksCreated);
            foreach (var t in tanks)
            {
                Console.WriteLine($"ID: {t.ID}, Model: {t.Model}, SerialNumber: {t.SerialNumber}, TankType: {t.TankType}");
            }
            break;

        case "2":
            if (manufacturers == null || tanks == null)
            {
                Console.WriteLine("Create instances first (1.)");
                break;
            }
            XmlHelper.SaveObjectsToXml(manufacturers, tanks, Constants.XmlFilePath);
            Console.WriteLine(Constants.ObjectsSavedToXml);
            break;

        case "3":
            if (File.Exists(Constants.XmlFilePath))
            {
                string xmlContent = File.ReadAllText(Constants.XmlFilePath);
                Console.WriteLine(xmlContent);
            }
            else
            {
                Console.WriteLine(Constants.NoXmlFileFound);
            }
            break;

        case "4":
            if (File.Exists(Constants.XmlFilePath))
            {
                var (manufacturersParsed, tanksParsed) = XmlHelper.ParseXmlToObjects(Constants.XmlFilePath);
                MenuHelper.DisplayObjectsWithContinuousNumbering(manufacturersParsed, tanksParsed);
            }
            break;

        case "5":
            if (File.Exists(Constants.XmlFilePath))
            {
                var modelValues = XmlHelper.ExtractModelValuesWithXDocument(Constants.XmlFilePath);
                Console.WriteLine(Constants.TankModelValuesXDocument);
                foreach (var model in modelValues)
                {
                    Console.WriteLine(model);
                }
            }
            break;

        case "6":
            if (File.Exists(Constants.XmlFilePath))
            {
                var modelValues = XmlHelper.ExtractModelValuesWithXmlDocument(Constants.XmlFilePath);
                Console.WriteLine(Constants.TankModelValuesXmlDocument);
                foreach (var model in modelValues)
                {
                    Console.WriteLine(model);
                }
            }
            break;

        case "7":
            MenuHelper.EditAttributeWithXDocument();
            break;

        case "8":
            MenuHelper.EditAttributeWithXmlDocument();
            break;

        case "9":
            Console.WriteLine("Exiting program.");
            return;

        default:
            Console.WriteLine("Invalid choice. Please select a valid option.");
            break;
    }
}
