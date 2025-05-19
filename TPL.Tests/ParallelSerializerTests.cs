using NUnit.Framework;
using TPL.Classes;
using System.Threading.Tasks;
using System.IO;

namespace TPL.Tests
{
    [TestFixture]
    public class ParallelSerializerTests
    {
        [Test]
        public void SerializeInTwoThreads_ShouldSerializeSuccessfully()
        {
            // Arrange
            var (tanks, manufacturers) = ClassFaker.CreateInstances();
            string fileNameTanks = "tanks.xml";
            string fileNameManufacturers = "manufacturers.xml";

            // Act
            ParallelSerializer.SerializeInTwoThreads(tanks, manufacturers, fileNameTanks, fileNameManufacturers);

            // Assert
            Assert.That(System.IO.File.Exists(fileNameTanks), "Tanks file should exist");
            Assert.That(System.IO.File.Exists(fileNameManufacturers), "Manufacturers file should exist");
        }
    }
}