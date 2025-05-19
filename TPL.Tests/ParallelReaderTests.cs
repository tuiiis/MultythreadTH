using TPL.Classes;

namespace TPL.Tests;

[TestFixture]
public class ParallelReaderTests
{
    private string fileNameMerged = Constants.FileNameMerged;

    [SetUp]
    public void SetUp()
    {
        if (!System.IO.File.Exists(fileNameMerged) || System.IO.File.ReadAllText(fileNameMerged).Length == 0)
        {
            System.IO.File.WriteAllText(fileNameMerged, "<root><test>Sample Content</test></root>");
        }
    }

    [Test]
    public void ReadSequentially_ShouldReturnFileContent()
    {
        string content = ParallelReader.ReadSequentially(fileNameMerged);

        Assert.That(content != null, "File content should not be null");
        Assert.That(content!.Length > 0, "File content should not be empty");
    }

    [Test]
    public void ReadInTwoThreads_ShouldReturnFileContent()
    {
        string content = ParallelReader.ReadInTwoThreads(fileNameMerged);
        Assert.That(content! != null, "File content should not be null");
        Assert.That(content!.Length > 0, "File content should not be empty");
    }

    [Test]
    public void ReadInTenThreads_ShouldReturnFileContent()
    {
        string content = ParallelReader.ReadInTenThreads(fileNameMerged);
        Assert.That(content != null, "File content should not be null");
        Assert.That(content!.Length > 0, "File content should not be empty");
    }
}