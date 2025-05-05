using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using Serialization.Models;

namespace Serialization.Helpers
{
    public static class XmlHelper
    {
        public static void SaveObjectsToXml(List<Manufacturer> manufacturers, List<Tank> tanks, string xmlFilePath)
        {
            var xDoc = new XDocument(
                new XElement("Root",
                    new XElement($"{nameof(Manufacturer)}s",
                        manufacturers.Select(m =>
                            new XElement(nameof(Manufacturer),
                                new XElement(nameof(Manufacturer.Name), m.Name),
                                new XElement(nameof(Manufacturer.Address), m.Address),
                                new XElement(nameof(Manufacturer.IsAChildCompany), m.IsAChildCompany)
                            )
                        )
                    ),
                    new XElement($"{nameof(Tank)}s",
                        tanks.Select(t =>
                            new XElement(nameof(Tank),
                                new XElement(nameof(Tank.ID), t.ID),
                                new XElement(nameof(Tank.Model), t.Model),
                                new XElement(nameof(Tank.SerialNumber), t.SerialNumber),
                                new XElement(nameof(Tank.TankType), t.TankType)
                            )
                        )
                    )
                )
            );
            xDoc.Save(xmlFilePath);
        }

        public static (List<Manufacturer>, List<Tank>) ParseXmlToObjects(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            var manufacturersParsed = doc.Descendants(nameof(Manufacturer))
                .Select(m => new Manufacturer(
                    m.Element(nameof(Manufacturer.Name))?.Value ?? string.Empty,
                    m.Element(nameof(Manufacturer.Address))?.Value ?? string.Empty,
                    bool.Parse(m.Element(nameof(Manufacturer.IsAChildCompany))?.Value ?? "false")
                )).ToList();

            var tanksParsed = doc.Descendants(nameof(Tank))
                .Select(t => new Tank(
                    int.Parse(t.Element(nameof(Tank.ID))?.Value ?? "0"),
                    t.Element(nameof(Tank.Model))?.Value ?? string.Empty,
                    t.Element(nameof(Tank.SerialNumber))?.Value ?? string.Empty,
                    (TankType)Enum.Parse(typeof(TankType), t.Element(nameof(Tank.TankType))?.Value ?? "StandardTank")
                )).ToList();

            return (manufacturersParsed, tanksParsed);
        }

        public static List<string> ExtractModelValuesWithXDocument(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc.Descendants(nameof(Tank))
                      .Select(t => t.Element(nameof(Tank.Model))?.Value ?? "Unknown Model")
                      .ToList();
        }

        public static List<string> ExtractModelValuesWithXmlDocument(string xmlFilePath)
        {
            XmlDocument doc = new();
            doc.Load(xmlFilePath);
            var modelNodes = doc.GetElementsByTagName(nameof(Tank.Model));
            var result = new List<string>();
            foreach (XmlNode node in modelNodes)
            {
                result.Add(node.InnerText);
            }
            return result;
        }

        public static bool EditAttributeWithXDocument(string xmlFilePath, string objectType, int elementIndex, string fieldName, string newValue, out string message)
        {
            if (!File.Exists(xmlFilePath))
            {
                message = "No XML file found.";
                return false;
            }

            XDocument doc = XDocument.Load(xmlFilePath);
            var elements = doc.Descendants(objectType).ToList();

            if (elements.Count == 0)
            {
                message = $"No {objectType} objects found in XML.";
                return false;
            }

            if (elementIndex < 1 || elementIndex > elements.Count)
            {
                message = "Invalid number.";
                return false;
            }

            var selectedElement = elements[elementIndex - 1];
            var targetElement = selectedElement.Element(fieldName);
            if (targetElement == null)
            {
                message = $"Field '{fieldName}' not found in {objectType}.";
                return false;
            }

            string currentType = GetElementType(targetElement);
            if (!ValidateNewValue(newValue, currentType))
            {
                message = $"Invalid value for type {currentType}";
                return false;
            }

            targetElement.Value = newValue;
            doc.Save(xmlFilePath);
            message = $"Updated {fieldName} to {newValue}";
            return true;
        }

        public static bool EditAttributeWithXmlDocument(string xmlFilePath, string objectType, int nodeIndex, string fieldName, string newValue, out string message)
        {
            if (!File.Exists(xmlFilePath))
            {
                message = "No XML file found.";
                return false;
            }

            XmlDocument doc = new();
            doc.Load(xmlFilePath);
            var nodes = doc.GetElementsByTagName(objectType);

            if (nodes.Count == 0)
            {
                message = $"No {objectType} objects found in XML.";
                return false;
            }

            if (nodeIndex < 1 || nodeIndex > nodes.Count)
            {
                message = "Invalid number.";
                return false;
            }

            var selectedNode = nodes[nodeIndex - 1];
            if (selectedNode == null)
            {
                message = "Selected node is null.";
                return false;
            }

            var targetNode = selectedNode[fieldName];
            if (targetNode == null)
            {
                message = $"Field '{fieldName}' not found in {objectType}.";
                return false;
            }

            string currentType = GetNodeType(targetNode);
            if (!ValidateNewValue(newValue, currentType))
            {
                message = $"Invalid value for type {currentType}";
                return false;
            }

            targetNode.InnerText = newValue;
            doc.Save(xmlFilePath);
            message = $"Updated {fieldName} to {newValue}";
            return true;
        }

        public static string GetElementSummary(XElement element)
        {
            var summaryBuilder = new StringBuilder();
            foreach (var child in element.Elements())
            {
                summaryBuilder.Append($"{child.Name}: {child.Value}, ");
            }
            return summaryBuilder.ToString().TrimEnd(',', ' ');
        }

        public static string GetNodeSummary(XmlNode node)
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

        public static string GetElementType(XElement element)
        {
            return element.Name.LocalName switch
            {
                nameof(Tank.ID) => "int",
                nameof(Manufacturer.IsAChildCompany) => "bool",
                nameof(Tank.TankType) => "enum",
                _ => "string"
            };
        }

        public static string GetNodeType(XmlNode node)
        {
            return node.Name switch
            {
                nameof(Tank.ID) => "int",
                nameof(Manufacturer.IsAChildCompany) => "bool",
                nameof(Tank.TankType) => "enum",
                _ => "string"
            };
        }

        public static bool ValidateNewValue(string value, string type)
        {
            return type switch
            {
                "int" => int.TryParse(value, out _),
                "bool" => bool.TryParse(value, out _),
                "enum" => Enum.IsDefined(typeof(TankType), value),
                _ => true
            };
        }

        // New methods for saving individual lists
        public static void SaveTanksToFile(List<Tank> tanks, string filePath)
        {
            var xDoc = new XDocument(
                new XElement("Root",
                    new XElement($"{nameof(Tank)}s", tanks.Select(t =>
                        new XElement(nameof(Tank),
                            new XElement(nameof(Tank.ID), t.ID),
                            new XElement(nameof(Tank.Model), t.Model),
                            new XElement(nameof(Tank.SerialNumber), t.SerialNumber),
                            new XElement(nameof(Tank.TankType), t.TankType)
                        )
                    )
                )
            )
            );

            xDoc.Save(filePath);
        }

        public static void SaveManufacturersToFile(List<Manufacturer> manufacturers, string filePath)
        {
            var xDoc = new XDocument(
                new XElement("Root",
                    new XElement($"{nameof(Manufacturer)}s", manufacturers.Select(m =>
                        new XElement(nameof(Manufacturer),
                            new XElement(nameof(Manufacturer.Name), m.Name),
                            new XElement(nameof(Manufacturer.Address), m.Address),
                            new XElement(nameof(Manufacturer.IsAChildCompany), m.IsAChildCompany)
                        )
                    )
                )
            )
            );
            xDoc.Save(filePath);
        }
    }
}
