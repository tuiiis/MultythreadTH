using Asynchrony.Classes;
using Asynchrony.Models;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Asynchrony.Tests
{
    [TestFixture]
    public class TankTests
    {
        private string _testDirectory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _testDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, TestConstants.TestDirectory);
            Directory.CreateDirectory(_testDirectory);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void CreateTanks_ShouldCreateCorrectNumberOfTanks()
        {
            // Arrange & Act
            var tanks = ClassFaker.CreateTanks(TestConstants.TestTankCount);

            // Assert
            Assert.That(tanks, Is.Not.Null);
            Assert.That(tanks.Count, Is.EqualTo(TestConstants.TestTankCount));
            Assert.That(tanks.All(t => t != null), Is.True);
        }


        [Test]
        public async Task ProcessXmlFilesAsync_ShouldCreateCorrectDictionary()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(TestConstants.TestTankCount);
            XMLManager.SaveGroupsToXML(tanks, TestConstants.TestFileCount);

            // Act
            var dictionary = await ClassManager.ProcessXmlFilesAsync(tanks);

            // Assert
            Assert.That(dictionary, Is.Not.Null);
            Assert.That(dictionary.Count, Is.GreaterThan(0));
            Assert.That(dictionary.All(kvp => kvp.Value.Count > 0), Is.True);
        }


        [Test]
        public void SaveToXML_ShouldCreateValidXMLFile()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(5);
            var filePath = Path.Combine(_testDirectory, "test_tanks.xml");

            // Act
            XMLManager.SaveToXML(filePath, tanks);

            // Assert
            Assert.That(File.Exists(filePath), Is.True);
            var doc = XDocument.Load(filePath);
            var savedTanks = doc.Descendants(nameof(Tank)).ToList();
            Assert.That(savedTanks.Count, Is.EqualTo(tanks.Count));
        }

        [Test]
        public void ProcessXmlFilesAsync_ShouldThrowException_WhenTanksIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await XMLManager.ProcessXmlFilesAsync(null));
            Assert.That(ex.Message, Does.Contain("generate tanks first"));
        }

        [Test]
        public void MergeTanksToFileAsync_ShouldThrowException_WhenDictionaryIsEmpty()
        {
            // Arrange
            var emptyDictionary = new ConcurrentDictionary<string, ConcurrentBag<Tank>>();

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await XMLManager.MergeTanksToFileAsync(emptyDictionary));
            Assert.That(ex.Message, Does.Contain("No tanks to merge"));
        }

        [Test]
        public async Task ReadFromXMLAsync_ShouldReadTanksCorrectly()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(5);
            var filePath = Path.Combine(_testDirectory, "test_read_tanks.xml");
            
            // Create XML with proper Manufacturer structure
            var xDoc = new XDocument(
                new XElement(nameof(Tank) + "s",
                    tanks.Select(t =>
                        new XElement(nameof(Tank),
                            new XElement(nameof(Tank.ID), t.ID),
                            new XElement(nameof(Tank.Model), t.Model),
                            new XElement(nameof(Tank.SerialNumber), t.SerialNumber),
                            new XElement(nameof(Tank.TankType), t.TankType),
                            new XElement(nameof(Tank.Manufacturer),
                                new XElement("Name", t.Manufacturer.Name),
                                new XElement("Address", t.Manufacturer.Address),
                                new XElement("IsAChildCompany", t.Manufacturer.IsAChildCompany)
                            )
                        )
                    )
                )
            );
            xDoc.Save(filePath);

            var progress = new Progress<int>(p => { });

            // Act
            var readTanks = await XMLManager.ReadFromXMLAsync(filePath, progress);

            // Assert
            Assert.That(readTanks, Is.Not.Null);
            Assert.That(readTanks.Count, Is.EqualTo(tanks.Count));
            for (int i = 0; i < tanks.Count; i++)
            {
                Assert.That(readTanks[i].ID, Is.EqualTo(tanks[i].ID));
                Assert.That(readTanks[i].Model, Is.EqualTo(tanks[i].Model));
                Assert.That(readTanks[i].SerialNumber, Is.EqualTo(tanks[i].SerialNumber));
                Assert.That(readTanks[i].TankType, Is.EqualTo(tanks[i].TankType));
                Assert.That(readTanks[i].Manufacturer.Name, Is.EqualTo(tanks[i].Manufacturer.Name));
                Assert.That(readTanks[i].Manufacturer.Address, Is.EqualTo(tanks[i].Manufacturer.Address));
                Assert.That(readTanks[i].Manufacturer.IsAChildCompany, Is.EqualTo(tanks[i].Manufacturer.IsAChildCompany));
            }
        }

        [Test]
        public void SaveGroupsToXML_ShouldCreateCorrectNumberOfFiles()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(10);
            var numberOfGroups = 3;

            // Act
            XMLManager.SaveGroupsToXML(tanks, numberOfGroups);

            // Assert
            var files = Directory.GetFiles(".", FileConstants.TankFilePattern);
            Assert.That(files.Length, Is.EqualTo(numberOfGroups));

            // Verify each file contains tanks
            foreach (var file in files)
            {
                var doc = XDocument.Load(file);
                var savedTanks = doc.Descendants(nameof(Tank)).ToList();
                Assert.That(savedTanks.Count, Is.GreaterThan(0));
            }
        }

        [Test]
        public void ReadFromXMLAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFile = Path.Combine(_testDirectory, "non_existent_file.xml");
            var progress = new Progress<int>(p => { });

            // Act & Assert
            var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await XMLManager.ReadFromXMLAsync(nonExistentFile, progress));
            Assert.That(ex.Message, Does.Contain("Could not find file"));
        }
    }
}