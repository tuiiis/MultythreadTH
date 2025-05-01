using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Serialization.Models;
using Serialization.Helpers;

namespace Serialization.Helpers
{
    public static class MenuHelper
    {
        public static void ShowMenu()
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
        }

        public static string ReadMenuChoice()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        public static string ReadObjectType()
        {
            Console.WriteLine($"Enter object type ({string.Join(" or ", Constants.ObjectTypes)}):");
            string? objectType = Console.ReadLine();
            if (string.IsNullOrEmpty(objectType) || Array.IndexOf(Constants.ObjectTypes, objectType) == -1)
            {
                Console.WriteLine($"Invalid object type. Must be one of: {string.Join(", ", Constants.ObjectTypes)}");
                return string.Empty;
            }
            return objectType;
        }

        public static int ReadIndex(string objectType, int count)
        {
            Console.WriteLine($"Enter {objectType} number to edit (1-{count}):");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > count)
            {
                Console.WriteLine("Invalid number.");
                return -1;
            }
            return index;
        }

        public static string ReadFieldName()
        {
            Console.WriteLine("Enter field name to edit:");
            string? fieldName = Console.ReadLine();
            if (string.IsNullOrEmpty(fieldName))
            {
                Console.WriteLine("Field name cannot be empty.");
                return string.Empty;
            }
            return fieldName;
        }

        public static string ReadNewValue(string fieldName, string currentValue, string currentType)
        {
            Console.WriteLine($"Current value of {fieldName}: {currentValue} (Type: {currentType})");
            Console.WriteLine("Enter new value:");
            return Console.ReadLine() ?? string.Empty;
        }

        public static void DisplayObjectsWithContinuousNumbering(List<Manufacturer> manufacturers, List<Tank> tanks)
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

        public static void EditAttributeWithXDocument()
        {
            if (!File.Exists(Constants.XmlFilePath))
            {
                Console.WriteLine("No XML file found.");
                return;
            }

            string objectType = ReadObjectType();
            if (string.IsNullOrEmpty(objectType)) return;

            var elements = XDocument.Load(Constants.XmlFilePath).Descendants(objectType).ToList();
            if (elements.Count == 0)
            {
                Console.WriteLine($"No {objectType} objects found in XML.");
                return;
            }

            Console.WriteLine($"Available {objectType} objects:");
            for (int i = 0; i < elements.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {XmlHelper.GetElementSummary(elements[i])}");
            }

            int elementIndex = ReadIndex(objectType, elements.Count);
            if (elementIndex == -1) return;

            string fieldName = ReadFieldName();
            if (string.IsNullOrEmpty(fieldName)) return;

            var targetElement = elements[elementIndex - 1].Element(fieldName);
            if (targetElement == null)
            {
                Console.WriteLine($"Field '{fieldName}' not found in {objectType}.");
                return;
            }

            string currentValue = targetElement.Value;
            string currentType = XmlHelper.GetElementType(targetElement);

            string newValue = ReadNewValue(fieldName, currentValue, currentType);
            if (string.IsNullOrEmpty(newValue)) return;

            if (XmlHelper.EditAttributeWithXDocument(Constants.XmlFilePath, objectType, elementIndex, fieldName, newValue, out string message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        public static void EditAttributeWithXmlDocument()
        {
            if (!File.Exists(Constants.XmlFilePath))
            {
                Console.WriteLine("No XML file found.");
                return;
            }

            string objectType = ReadObjectType();
            if (string.IsNullOrEmpty(objectType)) return;

            var doc = new XmlDocument();
            doc.Load(Constants.XmlFilePath);
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
                    Console.WriteLine($"{i + 1}. {XmlHelper.GetNodeSummary(nodes[i]!)}");
                }
            }

            int nodeIndex = ReadIndex(objectType, nodes.Count);
            if (nodeIndex == -1) return;

            var selectedNode = nodes[nodeIndex - 1];
            if (selectedNode == null)
            {
                Console.WriteLine("Selected node is null.");
                return;
            }

            string fieldName = ReadFieldName();
            if (string.IsNullOrEmpty(fieldName)) return;

            var targetNode = selectedNode[fieldName];
            if (targetNode == null)
            {
                Console.WriteLine($"Field '{fieldName}' not found in {objectType}.");
                return;
            }

            string currentValue = targetNode.InnerText;
            string currentType = XmlHelper.GetNodeType(targetNode);

            string newValue = ReadNewValue(fieldName, currentValue, currentType);
            if (string.IsNullOrEmpty(newValue)) return;

            if (XmlHelper.EditAttributeWithXmlDocument(Constants.XmlFilePath, objectType, nodeIndex, fieldName, newValue, out string message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
