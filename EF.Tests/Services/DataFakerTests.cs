using EF.Services;
using EF.Models;
using NUnit.Framework;

namespace EF.Tests.Services
{
    [TestFixture]
    public class DataFakerTests
    {
        [Test]
        public void CreateManufacturers_WithDefaultCount_ShouldReturnTenManufacturers()
        {
            // Act
            var manufacturers = DataFaker.CreateManufacturers();

            // Assert
            Assert.That(manufacturers, Is.Not.Null);
            Assert.That(manufacturers.Count, Is.EqualTo(10));
        }

        [Test]
        public void CreateManufacturers_WithCustomCount_ShouldReturnSpecifiedNumberOfManufacturers()
        {
            // Arrange
            var count = 5;

            // Act
            var manufacturers = DataFaker.CreateManufacturers(count);

            // Assert
            Assert.That(manufacturers, Is.Not.Null);
            Assert.That(manufacturers.Count, Is.EqualTo(count));
        }

        [Test]
        public void CreateTanks_WithDefaultCount_ShouldReturnTenTanks()
        {
            // Arrange
            var manufacturers = DataFaker.CreateManufacturers(3);

            // Act
            var tanks = DataFaker.CreateTanks(manufacturers);

            // Assert
            Assert.That(tanks, Is.Not.Null);
            Assert.That(tanks.Count, Is.EqualTo(10));
        }

        [Test]
        public void CreateTanks_ShouldGenerateValidTankData()
        {
            // Arrange
            var manufacturers = DataFaker.CreateManufacturers(3);

            // Act
            var tanks = DataFaker.CreateTanks(manufacturers, 5);

            // Assert
            foreach (var tank in tanks)
            {
                Assert.That(tank.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(tank.Model, Is.Not.Null);
                Assert.That(tank.Model, Is.Not.Empty);
                Assert.That(tank.SerialNumber, Is.Not.Null);
                Assert.That(tank.SerialNumber, Is.Not.Empty);
                Assert.That(tank.SerialNumber.Length, Is.EqualTo(10));
                Assert.That(tank.SerialNumber, Is.EqualTo(tank.SerialNumber.ToUpper()));
                Assert.That(Enum.IsDefined(typeof(TankType), tank.TankType), Is.True);
                Assert.That(tank.ManufacturerId, Is.Not.EqualTo(Guid.Empty));
                Assert.That(manufacturers.Any(m => m.Id == tank.ManufacturerId), Is.True);
            }
        }

        [Test]
        public void CreateTanks_WithEmptyManufacturersList_ShouldThrowException()
        {
            // Arrange
            var manufacturers = new List<Manufacturer>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => DataFaker.CreateTanks(manufacturers, 5));
        }

        [Test]
        public void CreateTanks_WithNullManufacturersList_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => DataFaker.CreateTanks(null!, 5));
        }
    }
} 