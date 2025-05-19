using TPL.Classes;

namespace TPL.Tests;

[TestFixture]
public class ParallelSerializerTests
{
    [Test]
    public void SerializeInTwoThreads_ShouldSerializeSuccessfully()
    {
        // Arrange
        var (tanks, manufacturers) = ClassFaker.CreateInstances();
        // Act
        ParallelSerializer.SerializeInTwoThreads(tanks, manufacturers, Constants.FileNameTanks, Constants.FileNameManufacturers);

        // Assert
        Assert.That(System.IO.File.Exists(Constants.FileNameTanks), "Tanks file should exist");
        Assert.That(System.IO.File.Exists(Constants.FileNameManufacturers), "Manufacturers file should exist");
    }
}