using EF.Services;
using EF.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EF.Tests.Services
{
    [TestFixture]
    public class TankDbContextTests
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
        public void TankDbContext_ShouldHaveTanksDbSet()
        {
            // Act & Assert
            Assert.That(_context.Tanks, Is.Not.Null);
            Assert.That(_context.Tanks, Is.InstanceOf<DbSet<Tank>>());
        }

        [Test]
        public void TankDbContext_ShouldHaveManufacturersDbSet()
        {
            // Act & Assert
            Assert.That(_context.Manufacturers, Is.Not.Null);
            Assert.That(_context.Manufacturers, Is.InstanceOf<DbSet<Manufacturer>>());
        }

        [Test]
        public async Task TankDbContext_ShouldCreateDatabase()
        {
            // Act
            await _context.Database.EnsureCreatedAsync();

            // Assert
            Assert.That(await _context.Database.CanConnectAsync(), Is.True);
        }

        [Test]
        public async Task TankDbContext_ShouldSaveAndRetrieveTank()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", false);
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var tank = new Tank("Test Tank", "SN001", TankType.Light, manufacturer.Id);

            // Act
            _context.Tanks.Add(tank);
            await _context.SaveChangesAsync();

            var retrievedTank = await _context.Tanks.FindAsync(tank.Id);

            // Assert
            Assert.That(retrievedTank, Is.Not.Null);
            Assert.That(retrievedTank.Model, Is.EqualTo("Test Tank"));
            Assert.That(retrievedTank.SerialNumber, Is.EqualTo("SN001"));
            Assert.That(retrievedTank.TankType, Is.EqualTo(TankType.Light));
            Assert.That(retrievedTank.ManufacturerId, Is.EqualTo(manufacturer.Id));
        }

        [Test]
        public async Task TankDbContext_ShouldSaveAndRetrieveManufacturer()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", true);

            // Act
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var retrievedManufacturer = await _context.Manufacturers.FindAsync(manufacturer.Id);

            // Assert
            Assert.That(retrievedManufacturer, Is.Not.Null);
            Assert.That(retrievedManufacturer.Name, Is.EqualTo("Test Manufacturer"));
            Assert.That(retrievedManufacturer.Address, Is.EqualTo("Test Address"));
            Assert.That(retrievedManufacturer.IsAChildCompany, Is.True);
        }

        [Test]
        public async Task TankDbContext_ShouldHandleTankManufacturerRelationship()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", false);
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var tank = new Tank("Test Tank", "SN001", TankType.Light, manufacturer.Id);
            _context.Tanks.Add(tank);
            await _context.SaveChangesAsync();

            // Act
            var tankWithManufacturer = await _context.Tanks
                .Include(t => t.Manufacturer)
                .FirstOrDefaultAsync(t => t.Id == tank.Id);

            // Assert
            Assert.That(tankWithManufacturer, Is.Not.Null);
            Assert.That(tankWithManufacturer.Manufacturer, Is.Not.Null);
            Assert.That(tankWithManufacturer.Manufacturer.Name, Is.EqualTo("Test Manufacturer"));
        }

        [Test]
        public async Task TankDbContext_ShouldHandleManufacturerTanksRelationship()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", false);
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var tank1 = new Tank("Tank 1", "SN001", TankType.Light, manufacturer.Id);
            var tank2 = new Tank("Tank 2", "SN002", TankType.Medium, manufacturer.Id);
            _context.Tanks.AddRange(tank1, tank2);
            await _context.SaveChangesAsync();

            // Act
            var manufacturerWithTanks = await _context.Manufacturers
                .Include(m => m.Tanks)
                .FirstOrDefaultAsync(m => m.Id == manufacturer.Id);

            // Assert
            Assert.That(manufacturerWithTanks, Is.Not.Null);
            Assert.That(manufacturerWithTanks.Tanks, Is.Not.Null);
            Assert.That(manufacturerWithTanks.Tanks.Count, Is.EqualTo(2));
            Assert.That(manufacturerWithTanks.Tanks.Any(t => t.Model == "Tank 1"), Is.True);
            Assert.That(manufacturerWithTanks.Tanks.Any(t => t.Model == "Tank 2"), Is.True);
        }

        [Test]
        public async Task TankDbContext_ShouldHandleCascadeDelete()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", false);
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var tank = new Tank("Test Tank", "SN001", TankType.Light, manufacturer.Id);
            _context.Tanks.Add(tank);
            await _context.SaveChangesAsync();

            // Act
            _context.Manufacturers.Remove(manufacturer);
            await _context.SaveChangesAsync();

            // Assert
            var deletedTank = await _context.Tanks.FindAsync(tank.Id);
            Assert.That(deletedTank, Is.Null);
        }

        [Test]
        public async Task TankDbContext_ShouldHandleTankTypeEnumConversion()
        {
            // Arrange
            var manufacturer = new Manufacturer("Test Manufacturer", "Test Address", false);
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            var lightTank = new Tank("Light Tank", "SN001", TankType.Light, manufacturer.Id);
            var mediumTank = new Tank("Medium Tank", "SN002", TankType.Medium, manufacturer.Id);
            var heavyTank = new Tank("Heavy Tank", "SN003", TankType.Heavy, manufacturer.Id);

            // Act
            _context.Tanks.AddRange(lightTank, mediumTank, heavyTank);
            await _context.SaveChangesAsync();

            var retrievedTanks = await _context.Tanks.ToListAsync();

            // Assert
            Assert.That(retrievedTanks.Count, Is.EqualTo(3));
            Assert.That(retrievedTanks.Any(t => t.TankType == TankType.Light), Is.True);
            Assert.That(retrievedTanks.Any(t => t.TankType == TankType.Medium), Is.True);
            Assert.That(retrievedTanks.Any(t => t.TankType == TankType.Heavy), Is.True);
        }

        [Test]
        public void TankDbContext_ShouldHaveCorrectEntityConfigurations()
        {
            // Act
            var model = _context.Model;

            // Assert
            var tankEntity = model.FindEntityType(typeof(Tank));
            var manufacturerEntity = model.FindEntityType(typeof(Manufacturer));

            Assert.That(tankEntity, Is.Not.Null);
            Assert.That(manufacturerEntity, Is.Not.Null);

            // Check that Tank has the correct primary key
            var tankPrimaryKey = tankEntity.FindPrimaryKey();
            Assert.That(tankPrimaryKey, Is.Not.Null);
            Assert.That(tankPrimaryKey.Properties.Count, Is.EqualTo(1));
            Assert.That(tankPrimaryKey.Properties.First().Name, Is.EqualTo("Id"));

            // Check that Manufacturer has the correct primary key
            var manufacturerPrimaryKey = manufacturerEntity.FindPrimaryKey();
            Assert.That(manufacturerPrimaryKey, Is.Not.Null);
            Assert.That(manufacturerPrimaryKey.Properties.Count, Is.EqualTo(1));
            Assert.That(manufacturerPrimaryKey.Properties.First().Name, Is.EqualTo("Id"));
        }
    }
} 