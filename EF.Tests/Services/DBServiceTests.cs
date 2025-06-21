using EF.Services;
using EF.Models;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Services
{
    [TestFixture]
    public class DBServiceTests
    {
        private TankDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TankDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public void Initialize_WithEmptyDatabase_ShouldCreateManufacturersAndTanks()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            Assert.That(_context.Manufacturers.Count(), Is.EqualTo(30));
            Assert.That(_context.Tanks.Count(), Is.EqualTo(30));
        }

        [Test]
        public void Initialize_WithExistingData_ShouldNotCreateAdditionalData()
        {
            // Arrange
            var existingManufacturer = new Manufacturer("Existing Manufacturer", "Existing Address", false);
            _context.Manufacturers.Add(existingManufacturer);
            _context.SaveChanges();

            var existingTank = new Tank("Existing Tank", "SN001", TankType.Light, existingManufacturer.Id);
            _context.Tanks.Add(existingTank);
            _context.SaveChanges();

            var initialManufacturerCount = _context.Manufacturers.Count();
            var initialTankCount = _context.Tanks.Count();

            // Act
            DBService.Initialize(_context);

            // Assert
            Assert.That(_context.Manufacturers.Count(), Is.EqualTo(initialManufacturerCount));
            Assert.That(_context.Tanks.Count(), Is.EqualTo(initialTankCount));
        }

        [Test]
        public void Initialize_ShouldCreateValidManufacturers()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var manufacturers = _context.Manufacturers.ToList();
            Assert.That(manufacturers.Count, Is.EqualTo(30));

            foreach (var manufacturer in manufacturers)
            {
                Assert.That(manufacturer.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(manufacturer.Name, Is.Not.Null);
                Assert.That(manufacturer.Name, Is.Not.Empty);
                Assert.That(manufacturer.Address, Is.Not.Null);
                Assert.That(manufacturer.Address, Is.Not.Empty);
                Assert.That(manufacturer.Tanks, Is.Not.Null);
            }
        }

        [Test]
        public void Initialize_ShouldCreateValidTanks()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var tanks = _context.Tanks.ToList();
            Assert.That(tanks.Count, Is.EqualTo(30));

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
            }
        }

        [Test]
        public void Initialize_ShouldCreateTanksWithValidManufacturerReferences()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var tanks = _context.Tanks.ToList();
            var manufacturers = _context.Manufacturers.ToList();
            var manufacturerIds = manufacturers.Select(m => m.Id).ToList();

            foreach (var tank in tanks)
            {
                Assert.That(manufacturerIds.Contains(tank.ManufacturerId), Is.True);
            }
        }

        [Test]
        public void Initialize_ShouldCreateUniqueManufacturerIds()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var manufacturers = _context.Manufacturers.ToList();
            var ids = manufacturers.Select(m => m.Id).ToList();
            Assert.That(ids.Count, Is.EqualTo(ids.Distinct().Count()));
        }

        [Test]
        public void Initialize_ShouldCreateUniqueTankIds()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var tanks = _context.Tanks.ToList();
            var ids = tanks.Select(t => t.Id).ToList();
            Assert.That(ids.Count, Is.EqualTo(ids.Distinct().Count()));
        }

        [Test]
        public void Initialize_ShouldCreateUniqueTankSerialNumbers()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var tanks = _context.Tanks.ToList();
            var serialNumbers = tanks.Select(t => t.SerialNumber).ToList();
            Assert.That(serialNumbers.Count, Is.EqualTo(serialNumbers.Distinct().Count()));
        }

        [Test]
        public void Initialize_ShouldCreateTanksWithAllTankTypes()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var tanks = _context.Tanks.ToList();
            Assert.That(tanks.Any(t => t.TankType == TankType.Light), Is.True);
            Assert.That(tanks.Any(t => t.TankType == TankType.Medium), Is.True);
            Assert.That(tanks.Any(t => t.TankType == TankType.Heavy), Is.True);
        }

        [Test]
        public void Initialize_ShouldCreateManufacturersWithBothChildAndParentCompanies()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            var manufacturers = _context.Manufacturers.ToList();
            Assert.That(manufacturers.Any(m => m.IsAChildCompany == true), Is.True);
            Assert.That(manufacturers.Any(m => m.IsAChildCompany == false), Is.True);
        }

        [Test]
        public void Initialize_ShouldHandleMultipleCallsGracefully()
        {
            // Act
            DBService.Initialize(_context);
            var firstCallManufacturerCount = _context.Manufacturers.Count();
            var firstCallTankCount = _context.Tanks.Count();

            DBService.Initialize(_context);
            var secondCallManufacturerCount = _context.Manufacturers.Count();
            var secondCallTankCount = _context.Tanks.Count();

            // Assert
            Assert.That(firstCallManufacturerCount, Is.EqualTo(secondCallManufacturerCount));
            Assert.That(firstCallTankCount, Is.EqualTo(secondCallTankCount));
        }

        [Test]
        public void Initialize_ShouldCreateDataInCorrectOrder()
        {
            // Act
            DBService.Initialize(_context);

            // Assert
            // Verify that manufacturers are created before tanks
            // This is implicit in the implementation, but we can verify the relationships
            var tanks = _context.Tanks.ToList();
            var manufacturers = _context.Manufacturers.ToList();

            foreach (var tank in tanks)
            {
                var manufacturer = manufacturers.FirstOrDefault(m => m.Id == tank.ManufacturerId);
                Assert.That(manufacturer, Is.Not.Null, $"Tank {tank.Id} references non-existent manufacturer {tank.ManufacturerId}");
            }
        }
    }
} 