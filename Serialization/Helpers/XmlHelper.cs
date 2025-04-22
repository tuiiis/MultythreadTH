using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using Serialization.Models;
using Serialization.Helpers;

namespace Serialization.Helpers
{
    public static class XmlHelper
    {
        public static void SaveObjectsToXml(List<Manufacturer> manufacturers, List<Tank> tanks, string xmlFilePath)
        {
            var xDoc = new XDocument(
                            new XElement(Constants.Element_Root,
                            new XElement(Constants.Element_Manufacturers, manufacturers.Select(m =>
                            new XElement(Constants.Element_Manufacturer,
                            new XElement(Constants.Element_Name, m.Name),
                            new XElement(Constants.Element_Address, m.Address),
                            new XElement(Constants.Element_IsAChildCompany, m.IsAChildCompany)
                        )
                    )),
                            new XElement(Constants.Element_Tanks, tanks.Select(t =>
                            new XElement(Constants.Element_Tank,
                            new XElement(Constants.Element_ID, t.ID),
                            new XElement(Constants.Element_Model, t.Model),
                            new XElement(Constants.Element_SerialNumber, t.SerialNumber),
                            new XElement(Constants.Element_TankType, t.TankType)
                        )
                    ))
                )
            );
            xDoc.Save(xmlFilePath);
        }

        public static (List<Manufacturer>, List<Tank>) ParseXmlToObjects(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            var manufacturersParsed = doc.Descendants(Constants.Element_Manufacturer)
                            .Select(m => new Manufacturer(
            m.Element(Constants.Element_Name)?.Value ?? string.Empty,
            m.Element(Constants.Element_Address)?.Value ?? string.Empty,
            bool.Parse(m.Element(Constants.Element_IsAChildCompany)?.Value ?? "false")
            )).ToList();

            var tanksParsed = doc.Descendants(Constants.Element_Tank)
                            .Select(t => new Tank(
            int.Parse(t.Element(Constants.Element_ID)?.Value ?? "0"),
            t.Element(Constants.Element_Model)?.Value ?? string.Empty,
            t.Element(Constants.Element_SerialNumber)?.Value ?? string.Empty,
            (TankType)Enum.Parse(typeof(TankType), t.Element(Constants.Element_TankType)?.Value ?? "StandardTank")
                            )).ToList();

            return (manufacturersParsed, tanksParsed);
        }

        public static List<string> ExtractModelValuesWithXDocument(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc.Descendants("Tank")
                      .Select(t => t.Element("Model")?.Value ?? "Unknown Model")
                      .ToList();
        }

        public static List<string> ExtractModelValuesWithXmlDocument(string xmlFilePath)
        {
            XmlDocument doc = new();
            doc.Load(xmlFilePath);
            var modelNodes = doc.GetElementsByTagName("Model");
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
                message = Constants.NoXmlFileFound;
                return false;
            }

            XDocument doc = XDocument.Load(xmlFilePath);
            var elements = doc.Descendants(objectType).ToList();

            if (elements.Count == 0)
            {
                message = string.Format(Constants.NoObjectsFoundInXml, objectType);
                return false;
            }

            if (elementIndex < 1 || elementIndex > elements.Count)
            {
                message = Constants.InvalidNumber;
                return false;
            }

            var selectedElement = elements[elementIndex - 1];
            var targetElement = selectedElement.Element(fieldName);
            if (targetElement == null)
            {
                message = string.Format(Constants.FieldNotFound, fieldName, objectType);
                return false;
            }

            string currentType = GetElementType(targetElement);
            if (!ValidateNewValue(newValue, currentType))
            {
                message = string.Format(Constants.InvalidValueForType, currentType);
                return false;
            }

            targetElement.Value = newValue;
            doc.Save(xmlFilePath);
            message = string.Format(Constants.UpdatedField, fieldName, newValue);
            return true;
        }

        public static bool EditAttributeWithXmlDocument(string xmlFilePath, string objectType, int nodeIndex, string fieldName, string newValue, out string message)
        {
            if (!File.Exists(xmlFilePath))
            {
                message = Constants.NoXmlFileFound;
                return false;
            }

            XmlDocument doc = new();
            doc.Load(xmlFilePath);
            var nodes = doc.GetElementsByTagName(objectType);

            if (nodes.Count == 0)
            {
                message = string.Format(Constants.NoObjectsFoundInXml, objectType);
                return false;
            }

            if (nodeIndex < 1 || nodeIndex > nodes.Count)
            {
                message = Constants.InvalidNumber;
                return false;
            }

            var selectedNode = nodes[nodeIndex - 1];
            if (selectedNode == null)
            {
                message = Constants.SelectedNodeIsNull;
                return false;
            }

            var targetNode = selectedNode[fieldName];
            if (targetNode == null)
            {
                message = string.Format(Constants.FieldNotFound, fieldName, objectType);
                return false;
            }

            string currentType = GetNodeType(targetNode);
            if (!ValidateNewValue(newValue, currentType))
            {
                message = string.Format(Constants.InvalidValueForType, currentType);
                return false;
            }

            targetNode.InnerText = newValue;
            doc.Save(xmlFilePath);
            message = string.Format(Constants.UpdatedField, fieldName, newValue);
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
                Constants.Element_ID => "int",
                Constants.Element_IsAChildCompany => "bool",
                Constants.Element_TankType => "enum",
                _ => "string"
            };
        }

        public static string GetNodeType(XmlNode node)
        {
            return node.Name switch
            {
                Constants.Element_ID => "int",
                Constants.Element_IsAChildCompany => "bool",
                Constants.Element_TankType => "enum",
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
                new XElement(Constants.Element_Root,
                    new XElement(Constants.Element_Tanks, tanks.Select(t =>
                        new XElement(Constants.Element_Tank,
                            new XElement(Constants.Element_ID, t.ID),
                            new XElement(Constants.Element_Model, t.Model),
                            new XElement(Constants.Element_SerialNumber, t.SerialNumber),
                            new XElement(Constants.Element_TankType, t.TankType)
                        )
                    )
                )
            );
            xDoc.Save(filePath);
        }

        public static void SaveManufacturersToFile(List<Manufacturer> manufacturers, string filePath)
        {
            var xDoc = new XDocument(
                new XElement(Constants.Element_Root,
                    new XElement(Constants.Element_Manufacturers, manufacturers.Select(m =>
                        new XElement(Constants.Element_Manufacturer,
                            new XElement(Constants.Element_Name, m.Name),
                            new XElement(Constants.Element_Address, m.Address),
                            new XElement(Constants.Element_IsAChildCompany, m.IsAChildCompany)
                        )
                    )
                )
            );
            xDoc.Save(filePath);
        }
    }
}
