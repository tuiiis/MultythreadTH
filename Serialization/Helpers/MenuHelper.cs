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
            Console.WriteLine(Constants.MenuPrompt);
            Console.WriteLine(Constants.MenuOption1);
            Console.WriteLine(Constants.MenuOption2);
            Console.WriteLine(Constants.MenuOption3);
            Console.WriteLine(Constants.MenuOption4);
            Console.WriteLine(Constants.MenuOption5);
            Console.WriteLine(Constants.MenuOption6);
            Console.WriteLine(Constants.MenuOption7);
            Console.WriteLine(Constants.MenuOption8);
            Console.WriteLine(Constants.MenuOption9);
        }

        public static string ReadMenuChoice()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        public static string ReadObjectType()
        {
            Console.WriteLine(string.Format(Constants.EnterObjectType, string.Join(" or ", Constants.ObjectTypes)));
            string? objectType = Console.ReadLine();
            if (string.IsNullOrEmpty(objectType) || Array.IndexOf(Constants.ObjectTypes, objectType) == -1)
            {
                Console.WriteLine(string.Format(Constants.InvalidObjectType, string.Join(", ", Constants.ObjectTypes)));
                return string.Empty;
            }
            return objectType;
        }

        public static int ReadIndex(string objectType, int count)
        {
            Console.WriteLine(string.Format(Constants.EnterObjectNumberToEdit, objectType, count));
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > count)
            {
                Console.WriteLine(Constants.InvalidNumber);
                return -1;
            }
            return index;
        }

        public static string ReadFieldName()
        {
            Console.WriteLine(Constants.EnterFieldNameToEdit);
            string? fieldName = Console.ReadLine();
            if (string.IsNullOrEmpty(fieldName))
            {
                Console.WriteLine(Constants.FieldNameCannotBeEmpty);
                return string.Empty;
            }
            return fieldName;
        }

        public static string ReadNewValue(string fieldName, string currentValue, string currentType)
        {
            Console.WriteLine(string.Format(Constants.CurrentValueOfField, fieldName, currentValue, currentType));
            Console.WriteLine(Constants.EnterNewValue);
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
                Console.WriteLine(Constants.NoXmlFileFound);
                return;
            }

            string objectType = ReadObjectType();
            if (string.IsNullOrEmpty(objectType)) return;

            var elements = XDocument.Load(Constants.XmlFilePath).Descendants(objectType).ToList();
            if (elements.Count == 0)
            {
                Console.WriteLine(string.Format(Constants.NoObjectsFoundInXml, objectType));
                return;
            }

            Console.WriteLine(string.Format(Constants.AvailableObjects, objectType));
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
                Console.WriteLine(string.Format(Constants.FieldNotFound, fieldName, objectType));
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
                Console.WriteLine(Constants.NoXmlFileFound);
                return;
            }

            string objectType = ReadObjectType();
            if (string.IsNullOrEmpty(objectType)) return;

            var doc = new XmlDocument();
            doc.Load(Constants.XmlFilePath);
            var nodes = doc.GetElementsByTagName(objectType);
            if (nodes.Count == 0)
            {
                Console.WriteLine(string.Format(Constants.NoObjectsFoundInXml, objectType));
                return;
            }

            Console.WriteLine(string.Format(Constants.AvailableObjects, objectType));
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
                Console.WriteLine(Constants.SelectedNodeIsNull);
                return;
            }

            string fieldName = ReadFieldName();
            if (string.IsNullOrEmpty(fieldName)) return;

            var targetNode = selectedNode[fieldName];
            if (targetNode == null)
            {
                Console.WriteLine(string.Format(Constants.FieldNotFound, fieldName, objectType));
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
