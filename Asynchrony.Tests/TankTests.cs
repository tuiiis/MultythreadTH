using Asynchrony.Classes;
using Asynchrony.Models;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
        public void SaveGroupsToXML_ShouldCreateCorrectNumberOfFiles()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(TestConstants.TestTankCount);
            var fileCount = TestConstants.TestFileCount;

            // Act
            XMLManager.SaveGroupsToXML(tanks, fileCount);

            // Assert
            var files = Directory.GetFiles(_testDirectory, FileConstants.TankFilePattern);
            Assert.That(files.Length, Is.EqualTo(fileCount));
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
        public async Task MergeTanksToFileAsync_ShouldCreateMergedFile()
        {
            // Arrange
            var tanks = ClassFaker.CreateTanks(TestConstants.TestTankCount);
            XMLManager.SaveGroupsToXML(tanks, TestConstants.TestFileCount);
            var dictionary = await ClassManager.ProcessXmlFilesAsync(tanks);

            // Act
            await ClassManager.MergeTanksToFileAsync(dictionary);

            // Assert
            var mergedFile = Path.Combine(_testDirectory, FileConstants.MergedTanksFile);
            Assert.That(File.Exists(mergedFile), Is.True);
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
    }
} 