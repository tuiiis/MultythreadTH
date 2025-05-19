using TPL.Classes;

namespace TPL.Tests
{
    [TestFixture]
    public class MergeManagerTests
    {
        [Test]
        public async Task MergeFilesAsync_ShouldMergeFilesSuccessfully()
        {
            await MergeManager.MergeFilesAsync(Constants.FileNameTanks, Constants.FileNameManufacturers, Constants.FileNameMerged);
            Assert.That(System.IO.File.Exists(Constants.FileNameMerged), "Merged file should exist");
        }
    }
}