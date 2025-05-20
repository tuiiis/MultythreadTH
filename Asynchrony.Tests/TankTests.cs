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
        public void XMLManager_Sorting_ShouldToggleCorrectly()
        {
            // Arrange
            var xmlManager = new XMLManager();
            var tanks = ClassFaker.CreateTanks(TestConstants.TestTankCount);
            var dictionary = new ConcurrentDictionary<string, ConcurrentBag<Tank>>();

            // Act & Assert
            Assert.That(xmlManager.IsSortingEnabled, Is.False);

            xmlManager.StartSorting(dictionary);
            Assert.That(xmlManager.IsSortingEnabled, Is.True);

            xmlManager.StopSorting();
            Assert.That(xmlManager.IsSortingEnabled, Is.False);
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
    }
}