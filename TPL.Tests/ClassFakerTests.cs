namespace TPL.Tests;

[TestFixture]
public class ClassFakerTests
{
    [Test]
    public void CreateInstances_ShouldReturnNonEmptyLists()
    {
        // Arrange
        var (tanks, manufacturers) = Classes.ClassFaker.CreateInstances();

        // Act
        var tanksCount = tanks?.Count ?? 0;
        var manufacturersCount = manufacturers?.Count ?? 0;

        // Assert
        Assert.That(tanksCount, Is.GreaterThan(0), "Tanks list should not be empty");
        Assert.That(manufacturersCount, Is.GreaterThan(0), "Manufacturers list should not be empty");
    }
}