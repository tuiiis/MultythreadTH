using Serialization.Classes;
using System.Text;
using System.Xml.Linq;
using System.Xml;

// Store class names in a list
List<string> objectTypes = ["Manufacturer", "Tank"];

List<Manufacturer>? manufacturers = null;
List<Tank>? tanks = null;
string xmlFilePath = "objects.xml";

while (true)
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Create 10 Manufacturers and 10 Tanks");
    Console.WriteLine("2. Save objects to XML");
    Console.WriteLine("3. Display XML as text");
    Console.WriteLine("4. Parse XML to objects");
    Console.WriteLine("5. Extract 'Model' values with XDocument");
    Console.WriteLine("6. Extract 'Model' values with XMLDocument");
    Console.WriteLine("7. Edit attribute by field name (XDocument)");
    Console.WriteLine("8. Edit attribute by field name (XMLDocument)");
    Console.WriteLine("9. Exit program");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            manufacturers = ClassFaker.ManufacturerFaker.Generate(10);
            tanks = ClassFaker.TankFaker.Generate(10);

            Console.WriteLine("Manufacturers created:");
            foreach (var m in manufacturers)
            {
                Console.WriteLine($"Name: {m.Name}, Address: {m.Address}, IsAChildCompany: {m.IsAChildCompany}");
            }

            Console.WriteLine("Tanks created:");
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
            var xDoc = new XDocument(
                new XElement("Root",
                    new XElement("Manufacturers", manufacturers.Select(m =>
                        new XElement("Manufacturer",
                            new XElement("Name", m.Name),
                            new XElement("Address", m.Address),
                            new XElement("IsAChildCompany", m.IsAChildCompany)
                        )
                    )),
                    new XElement("Tanks", tanks.Select(t =>
                        new XElement("Tank",
                            new XElement("ID", t.ID),
                            new XElement("Model", t.Model),
                            new XElement("SerialNumber", t.SerialNumber),
                            new XElement("TankType", t.TankType)
                        )
                    ))
                )
            );

            xDoc.Save(xmlFilePath);
            Console.WriteLine("Objects saved to XML.");
            break;

        case "3":
            if (File.Exists(xmlFilePath))
            {
                string xmlContent = File.ReadAllText(xmlFilePath);
                Console.WriteLine(xmlContent);
            }
            else
            {
                Console.WriteLine("No XML file found.");
            }
            break;

        case "4":
            if (File.Exists(xmlFilePath))
            {
                XDocument doc = XDocument.Load(xmlFilePath);

                var manufacturersParsed = doc.Descendants("Manufacturer")
                    .Select(m => new Manufacturer(
                        m.Element("Name")?.Value ?? string.Empty,
                        m.Element("Address")?.Value ?? string.Empty,
                        bool.Parse(m.Element("IsAChildCompany")?.Value ?? "false")
                    )).ToList();

                var tanksParsed = doc.Descendants("Tank")
                    .Select(t => new Tank(
                        int.Parse(t.Element("ID")?.Value ?? "0"),
                        t.Element("Model")?.Value ?? string.Empty,
                        t.Element("SerialNumber")?.Value ?? string.Empty,
                        (TankType)Enum.Parse(typeof(TankType), t.Element("TankType")?.Value ?? "StandardTank")
                    )).ToList();

                DisplayObjectsWithContinuousNumbering(manufacturersParsed, tanksParsed);
            }
            break;

        case "5":
            if (File.Exists(xmlFilePath))
            {
                XDocument doc = XDocument.Load(xmlFilePath);
                var modelValues = doc.Descendants("Tank")
                                     .Select(t => t.Element("Model")?.Value ?? "Unknown Model")
                                     .ToList();

                Console.WriteLine("Tank Model values (XDocument):");
                foreach (var model in modelValues)
                {
                    Console.WriteLine(model);
                }
            }
            break;

        case "6":
            if (File.Exists(xmlFilePath))
            {
                XmlDocument doc = new();
                doc.Load(xmlFilePath);
                var modelNodes = doc.GetElementsByTagName("Model");

                Console.WriteLine("Tank Model values (XMLDocument):");
                foreach (XmlNode node in modelNodes)
                {
                    Console.WriteLine(node.InnerText);
                }
            }
            break;

        case "7":
            EditAttributeWithXDocument();
            break;

        case "8":
            EditAttributeWithXMLDocument();
            break;

        case "9":
            Console.WriteLine("Exiting program.");
            return;

        default:
            Console.WriteLine("Invalid choice. Please select a valid option.");
            break;
    }
}

void DisplayObjectsWithContinuousNumbering(List<Manufacturer> manufacturers, List<Tank> tanks)
{
    int index = 1;

    Console.WriteLine("Manufacturers:");
    foreach (var m in manufacturers)
    {
        Console.WriteLine($"{index}. Name: {m.Name}, Address: {m.Address}, IsAChildCompany: {m.IsAChildCompany}");
        index++;
    }

    Console.WriteLine("Tanks:");
    foreach (var t in tanks)
    {
        Console.WriteLine($"{index}. ID: {t.ID}, Model: {t.Model}, SerialNumber: {t.SerialNumber}, TankType: {t.TankType}");
        index++;
    }
}

void EditAttributeWithXDocument()
{
    if (!File.Exists(xmlFilePath))
    {
        Console.WriteLine("No XML file found.");
        return;
    }

    Console.WriteLine($"Enter object type ({string.Join(" or ", objectTypes)}):");
    string? objectType = Console.ReadLine();

    if (string.IsNullOrEmpty(objectType) || !objectTypes.Contains(objectType))
    {
        Console.WriteLine($"Invalid object type. Must be one of: {string.Join(", ", objectTypes)}");
        return;
    }

    XDocument doc = XDocument.Load(xmlFilePath);
    var elements = doc.Descendants(objectType).ToList();

    if (elements.Count == 0)
    {
        Console.WriteLine($"No {objectType} objects found in XML.");
        return;
    }

    Console.WriteLine($"Available {objectType} objects:");
    for (int i = 0; i < elements.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {GetElementSummary(elements[i])}");
    }

    Console.WriteLine($"Enter {objectType} number to edit (1-{elements.Count}):");
    if (!int.TryParse(Console.ReadLine(), out int elementIndex) || elementIndex < 1 || elementIndex > elements.Count)
    {
        Console.WriteLine("Invalid number.");
        return;
    }

    var selectedElement = elements[elementIndex - 1];

    Console.WriteLine("Enter field name to edit:");
    string? fieldName = Console.ReadLine();

    if (string.IsNullOrEmpty(fieldName))
    {
        Console.WriteLine("Field name cannot be empty.");
        return;
    }

    var targetElement = selectedElement.Element(fieldName);
    if (targetElement == null)
    {
        Console.WriteLine($"Field '{fieldName}' not found in {objectType}.");
        return;
    }

    string currentValue = targetElement.Value;
    string currentType = GetElementType(targetElement);

    Console.WriteLine($"Current value of {fieldName}: {currentValue} (Type: {currentType})");
    Console.WriteLine("Enter new value:");
    string? newValue = Console.ReadLine();

    if (newValue != null && ValidateNewValue(newValue, currentType))
    {
        targetElement.Value = newValue;
        doc.Save(xmlFilePath);
        Console.WriteLine($"Updated {fieldName} to {newValue}");
    }
    else
    {
        Console.WriteLine($"Invalid value for type {currentType}");
    }
}

void EditAttributeWithXMLDocument()
{
    if (!File.Exists(xmlFilePath))
    {
        Console.WriteLine("No XML file found.");
        return;
    }

    Console.WriteLine($"Enter object type ({string.Join(" or ", objectTypes)}):");
    string? objectType = Console.ReadLine();

    if (string.IsNullOrEmpty(objectType) || !objectTypes.Contains(objectType))
    {
        Console.WriteLine($"Invalid object type. Must be one of: {string.Join(", ", objectTypes)}");
        return;
    }

    XmlDocument doc = new();
    doc.Load(xmlFilePath);
    var nodes = doc.GetElementsByTagName(objectType);

    if (nodes.Count == 0)
    {
        Console.WriteLine($"No {objectType} objects found in XML.");
        return;
    }

    Console.WriteLine($"Available {objectType} objects:");
    for (int i = 0; i < nodes.Count; i++)
    {
        if (nodes[i] != null)
        {
            Console.WriteLine($"{i + 1}. {GetNodeSummary(nodes[i]!)}");
        }
    }

    Console.WriteLine($"Enter {objectType} number to edit (1-{nodes.Count}):");
    if (!int.TryParse(Console.ReadLine(), out int nodeIndex) || nodeIndex < 1 || nodeIndex > nodes.Count)
    {
        Console.WriteLine("Invalid number.");
        return;
    }

    var selectedNode = nodes[nodeIndex - 1];
    if (selectedNode == null)
    {
        Console.WriteLine("Selected node is null.");
        return;
    }

    Console.WriteLine("Enter field name to edit:");
    string? fieldName = Console.ReadLine();

    if (string.IsNullOrEmpty(fieldName))
    {
        Console.WriteLine("Field name cannot be empty.");
        return;
    }

    var targetNode = selectedNode[fieldName];
    if (targetNode == null)
    {
        Console.WriteLine($"Field '{fieldName}' not found in {objectType}.");
        return;
    }

    string currentValue = targetNode.InnerText;
    string currentType = GetNodeType(targetNode);

    Console.WriteLine($"Current value of {fieldName}: {currentValue} (Type: {currentType})");
    Console.WriteLine("Enter new value:");
    string? newValue = Console.ReadLine();

    if (newValue != null && ValidateNewValue(newValue, currentType))
    {
        targetNode.InnerText = newValue;
        doc.Save(xmlFilePath);
        Console.WriteLine($"Updated {fieldName} to {newValue}");
    }
    else
    {
        Console.WriteLine($"Invalid value for type {currentType}");
    }
}

string GetElementSummary(XElement element)
{
    var summaryBuilder = new StringBuilder();
    foreach (var child in element.Elements())
    {
        summaryBuilder.Append($"{child.Name}: {child.Value}, ");
    }
    return summaryBuilder.ToString().TrimEnd(',', ' ');
}

string GetNodeSummary(XmlNode node)
{
    var summaryBuilder = new StringBuilder();
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType == XmlNodeType.Element)
        {
            summaryBuilder.Append($"{child.Name}: {child.InnerText}, ");
        }
    }
    return summaryBuilder.ToString().TrimEnd(',', ' ');
}

string GetElementType(XElement element)
{
    return element.Name.LocalName switch
    {
        "ID" => "int",
        "IsAChildCompany" => "bool",
        "TankType" => "enum",
        _ => "string"
    };
}

string GetNodeType(XmlNode node)
{
    return node.Name switch
    {
        "ID" => "int",
        "IsAChildCompany" => "bool",
        "TankType" => "enum",
        _ => "string"
    };
}

bool ValidateNewValue(string value, string type)
{
    return type switch
    {
        "int" => int.TryParse(value, out _),
        "bool" => bool.TryParse(value, out _),
        "enum" => Enum.IsDefined(typeof(TankType), value),
        _ => true
    };
}