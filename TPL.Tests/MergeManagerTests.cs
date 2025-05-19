using NUnit.Framework;
using TPL.Classes;
using System.Threading.Tasks;

namespace TPL.Tests
{
    [TestFixture]
    public class MergeManagerTests
    {
        [Test]
        public async Task MergeFilesAsync_ShouldMergeFilesSuccessfully()
        {

            string fileNameTanks = "tanks.xml";
            string fileNameManufacturers = "manufacturers.xml";
            string fileNameMerged = "merged.xml";

            // Act
            await MergeManager.MergeFilesAsync(fileNameTanks, fileNameManufacturers, fileNameMerged);

            // Assert
            Assert.That(System.IO.File.Exists(fileNameMerged), "Merged file should exist");
        }
    }
}