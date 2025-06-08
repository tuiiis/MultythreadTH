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
            var tanks = ClassFaker.CreateTanks(5);

            // Assert
            Assert.That(tanks.Count, Is.EqualTo(5));
        }

        [Test]
        public void CreateTanks_ShouldCreateValidTankObjects()
        {
            // Arrange & Act
            var tanks = ClassFaker.CreateTanks(1);

            // Assert
            var tank = tanks[0];
            Assert.That(tank.ID, Is.GreaterThanOrEqualTo(0));
            Assert.That(tank.Model, Is.Not.Empty);
            Assert.That(tank.SerialNumber, Is.Not.Empty);
            Assert.That(tank.Manufacturer, Is.Not.Null);
            Assert.That(tank.Manufacturer.Name, Is.Not.Empty);
            Assert.That(tank.Manufacturer.Address, Is.Not.Empty);
        }

        [Test]
        public void CreateTanks_ShouldCreateUniqueTanks()
        {
            // Arrange & Act
            var tanks = ClassFaker.CreateTanks(10);

            // Assert
            var uniqueIds = tanks.Select(t => t.ID).Distinct().ToList();
            Assert.That(uniqueIds.Count, Is.EqualTo(tanks.Count));
        }

        [Test]
        public void CreateTanks_ShouldCreateTanksWithValidTankTypes()
        {
            // Arrange & Act
            var tanks = ClassFaker.CreateTanks(10);

            // Assert
            Assert.That(tanks.All(t => Enum.IsDefined(typeof(TankType), t.TankType)), Is.True);
        }

        [Test]
        public void SaveToXML_ShouldCreateFile()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(1);
            var filePath = Path.Combine(_testDirectory, "test_tank.xml");

            // Act
            XMLManager.SaveToXML(filePath, tanks);

            // Assert
            Assert.That(File.Exists(filePath), Is.True);
        }

        [Test]
        public void SaveToXML_ShouldCreateValidXMLStructure()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(1);
            var filePath = Path.Combine(_testDirectory, "test_tank.xml");

            // Act
            XMLManager.SaveToXML(filePath, tanks);

            // Assert
            var doc = XDocument.Load(filePath);
            var tankElement = doc.Descendants(nameof(Tank)).First();
            Assert.That(tankElement.Element(nameof(Tank.ID)), Is.Not.Null);
            Assert.That(tankElement.Element(nameof(Tank.Model)), Is.Not.Null);
            Assert.That(tankElement.Element(nameof(Tank.SerialNumber)), Is.Not.Null);
            Assert.That(tankElement.Element(nameof(Tank.TankType)), Is.Not.Null);
            Assert.That(tankElement.Element(nameof(Tank.Manufacturer)), Is.Not.Null);
        }

        [Test]
        public void SaveToXML_ShouldHandleEmptyTankList()
        {
            // Arrange
            var tanks = new List<Tank>();
            var filePath = Path.Combine(_testDirectory, "empty_tanks.xml");

            // Act
            XMLManager.SaveToXML(filePath, tanks);

            // Assert
            var doc = XDocument.Load(filePath);
            Assert.That(doc.Descendants(nameof(Tank)).Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task ReadFromXMLAsync_ShouldReadTankData()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(1);
            var filePath = Path.Combine(_testDirectory, "test_read_tank.xml");
            XMLManager.SaveToXML(filePath, tanks);
            var progress = new Progress<int>(p => { });

            // Act
            var readTanks = await XMLManager.ReadFromXMLAsync(filePath, progress);

            // Assert
            Assert.That(readTanks.Count, Is.EqualTo(1));
            Assert.That(readTanks[0].ID, Is.EqualTo(tanks[0].ID));
            Assert.That(readTanks[0].Model, Is.EqualTo(tanks[0].Model));
            Assert.That(readTanks[0].SerialNumber, Is.EqualTo(tanks[0].SerialNumber));
        }

        [Test]
        public void SaveGroupsToXML_ShouldCreateCorrectNumberOfFiles()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(4);
            var numberOfGroups = 4;

            // Act
            XMLManager.SaveGroupsToXML(tanks, numberOfGroups);

            // Assert
            var files = Directory.GetFiles(".", FileConstants.TankFilePattern);
            Assert.That(files.Length, Is.EqualTo(numberOfGroups));
        }

        [Test]
        public void SaveGroupsToXML_ShouldDistributeTanksEvenly()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(8);
            var numberOfGroups = 4;

            // Act
            XMLManager.SaveGroupsToXML(tanks, numberOfGroups);

            // Assert
            var files = Directory.GetFiles(".", FileConstants.TankFilePattern);
            foreach (var file in files)
            {
                var doc = XDocument.Load(file);
                var tanksInFile = doc.Descendants(nameof(Tank)).Count();
                Assert.That(tanksInFile, Is.EqualTo(2)); // 8 tanks / 4 groups = 2 tanks per group
            }
        }

        [Test]
        public void ProcessXmlFilesAsync_ShouldThrowException_WhenTanksIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await ClassManager.ProcessXmlFilesAsync(null));
            Assert.That(ex.Message, Does.Contain("generate tanks first"));
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

        [Test]
        public void SaveToXML_ShouldThrowException_WhenFilePathIsInvalid()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(1);
            var invalidPath = "invalid/path/test.xml";

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => 
                XMLManager.SaveToXML(invalidPath, tanks));
        }
    }
}