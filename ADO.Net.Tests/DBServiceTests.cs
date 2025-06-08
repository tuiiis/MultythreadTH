using ADO.Net.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace ADO.Net.Tests;
public class DBServiceTests
{
    private DBService _dbService = null!;
    private NpgsqlConnection _connection = null!;
    private IConfiguration _configuration = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = _configuration.GetConnectionString("MyDatabase");
        _connection = new NpgsqlConnection(connectionString);
        _dbService = new DBService(_connection);
    }

    [SetUp]
    public void Setup()
    {
        // Ensure tables are created before each test
        _dbService.CreateTables();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up data after each test
        using var cmd = new NpgsqlCommand("DELETE FROM Tank; DELETE FROM Manufacturer;", _connection);
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            _connection.Open();
        }
        cmd.ExecuteNonQuery();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _connection.Dispose();
    }

    [Test]
    public void CreateTables_ShouldCreateTablesSuccessfully()
    {
        // Act
        _dbService.CreateTables();

        // Assert
        using var cmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'manufacturer');" +
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'tank');",
            _connection);

        if (_connection.State != System.Data.ConnectionState.Open)
        {
            _connection.Open();
        }

        using var reader = cmd.ExecuteReader();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetBoolean(0), Is.True, "Manufacturer table should exist");

        Assert.That(reader.NextResult(), Is.True);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetBoolean(0), Is.True, "Tank table should exist");
    }

    [Test]
    public async Task InsertManufacturerAsync_ShouldInsertManufacturerAndReturnId()
    {
        // Arrange
        var manufacturer = new Manufacturer("Test Company", "Test Address", true);

        // Act
        var id = await _dbService.InsertManufacturerAsync(manufacturer);

        // Assert
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(id, Is.EqualTo(manufacturer.Id));

        // Verify in database
        using var cmd = new NpgsqlCommand(
            "SELECT Name, Address, IsAChildCompany FROM Manufacturer WHERE Id = @id",
            _connection);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetString(0), Is.EqualTo("Test Company"));
        Assert.That(reader.GetString(1), Is.EqualTo("Test Address"));
        Assert.That(reader.GetBoolean(2), Is.True);
    }

    [Test]
    public async Task InsertTankAsync_ShouldInsertTankSuccessfully()
    {
        // Arrange
        var manufacturer = new Manufacturer("Test Company", "Test Address", true);
        await _dbService.InsertManufacturerAsync(manufacturer);
        var tank = new Tank("Test Model", "SN123", TankType.Medium, manufacturer.Id);

        // Act
        await _dbService.InsertTankAsync(tank);

        // Assert
        using var cmd = new NpgsqlCommand(
            "SELECT Model, SerialNumber, TankType, ManufacturerId FROM Tank WHERE Id = @id",
            _connection);
        cmd.Parameters.AddWithValue("id", tank.Id);

        using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetString(0), Is.EqualTo("Test Model"));
        Assert.That(reader.GetString(1), Is.EqualTo("SN123"));
        Assert.That(reader.GetInt32(2), Is.EqualTo((int)TankType.Medium));
        Assert.That(reader.GetGuid(3), Is.EqualTo(manufacturer.Id));
    }

    [Test]
    public async Task GetTanksByManufacturerIdAsync_ShouldReturnCorrectTanks()
    {
        // Arrange
        var manufacturer = new Manufacturer("Test Company", "Test Address", true);
        await _dbService.InsertManufacturerAsync(manufacturer);

        var tank1 = new Tank("Model1", "SN1", TankType.Light, manufacturer.Id);
        var tank2 = new Tank("Model2", "SN2", TankType.Heavy, manufacturer.Id);

        await _dbService.InsertTankAsync(tank1);
        await _dbService.InsertTankAsync(tank2);

        // Act
        var tanks = await _dbService.GetTanksByManufacturerIdAsync(manufacturer.Id);

        // Assert
        Assert.That(tanks, Has.Count.EqualTo(2));
        Assert.That(tanks.Any(t => t.Model == "Model1" && t.SerialNumber == "SN1"), Is.True);
        Assert.That(tanks.Any(t => t.Model == "Model2" && t.SerialNumber == "SN2"), Is.True);

        // Verify manufacturer details in returned tanks
        foreach (var tank in tanks)
        {
            Assert.That(tank.Manufacturer, Is.Not.Null);
            Assert.That(tank.Manufacturer!.Name, Is.EqualTo("Test Company"));
            Assert.That(tank.Manufacturer.Address, Is.EqualTo("Test Address"));
            Assert.That(tank.Manufacturer.IsAChildCompany, Is.True);
        }
    }

    [Test]
    public async Task GetTanksByManufacturerIdAsync_WithInvalidId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var tanks = await _dbService.GetTanksByManufacturerIdAsync(invalidId);

        // Assert
        Assert.That(tanks, Is.Empty);
    }

    [Test]
    public async Task DataAdder_ShouldAddCorrectNumberOfRecords()
    {
        // Act
        await _dbService.DataAdder();

        // Assert
        using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM Manufacturer; SELECT COUNT(*) FROM Tank;",
            _connection);

        using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(30), "Should have 30 manufacturers");

        Assert.That(reader.NextResult(), Is.True);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(30), "Should have 30 tanks");
    }

    [Test]
    public async Task InsertTankAsync_WithInvalidManufacturerId_ShouldThrowException()
    {
        // Arrange
        var invalidManufacturerId = Guid.NewGuid();
        var tank = new Tank("Test Model", "SN123", TankType.Medium, invalidManufacturerId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<PostgresException>(async () =>
            await _dbService.InsertTankAsync(tank));
        Assert.That(ex!.Message, Does.Contain("violates foreign key constraint"));
    }

    [Test]
    public async Task GetTanksByManufacturerIdAsync_ShouldReturnTanksInCorrectOrder()
    {
        // Arrange
        var manufacturer = new Manufacturer("Test Company", "Test Address", true);
        await _dbService.InsertManufacturerAsync(manufacturer);

        var tank1 = new Tank("Model1", "SN1", TankType.Light, manufacturer.Id);
        var tank2 = new Tank("Model2", "SN2", TankType.Medium, manufacturer.Id);
        var tank3 = new Tank("Model3", "SN3", TankType.Heavy, manufacturer.Id);

        await _dbService.InsertTankAsync(tank1);
        await _dbService.InsertTankAsync(tank2);
        await _dbService.InsertTankAsync(tank3);

        // Act
        var tanks = await _dbService.GetTanksByManufacturerIdAsync(manufacturer.Id);

        // Assert
        Assert.That(tanks, Has.Count.EqualTo(3));
        Assert.That(tanks[0].Model, Is.EqualTo("Model1"));
        Assert.That(tanks[1].Model, Is.EqualTo("Model2"));
        Assert.That(tanks[2].Model, Is.EqualTo("Model3"));
    }

    [Test]
    public async Task DataAdder_ShouldCreateUniqueRecords()
    {
        // Act
        await _dbService.DataAdder();

        // Assert
        using var cmd = new NpgsqlCommand(
            "SELECT COUNT(DISTINCT Name) FROM Manufacturer; " +
            "SELECT COUNT(DISTINCT Model) FROM Tank;",
            _connection);

        using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(30), "Should have 30 unique manufacturer names");

        Assert.That(reader.NextResult(), Is.True);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(30), "Should have 30 unique tank models");
    }

    [Test]
    public async Task GetTanksByManufacturerIdAsync_ShouldHandleMultipleManufacturers()
    {
        // Arrange
        var manufacturer1 = new Manufacturer("Company1", "Address1", true);
        var manufacturer2 = new Manufacturer("Company2", "Address2", false);
        await _dbService.InsertManufacturerAsync(manufacturer1);
        await _dbService.InsertManufacturerAsync(manufacturer2);

        var tank1 = new Tank("Model1", "SN1", TankType.Light, manufacturer1.Id);
        var tank2 = new Tank("Model2", "SN2", TankType.Medium, manufacturer2.Id);
        var tank3 = new Tank("Model3", "SN3", TankType.Heavy, manufacturer1.Id);

        await _dbService.InsertTankAsync(tank1);
        await _dbService.InsertTankAsync(tank2);
        await _dbService.InsertTankAsync(tank3);

        // Act
        var tanks1 = await _dbService.GetTanksByManufacturerIdAsync(manufacturer1.Id);
        var tanks2 = await _dbService.GetTanksByManufacturerIdAsync(manufacturer2.Id);

        // Assert
        Assert.That(tanks1, Has.Count.EqualTo(2));
        Assert.That(tanks2, Has.Count.EqualTo(1));

        Assert.That(tanks1.All(t => t.ManufacturerId == manufacturer1.Id), Is.True);
        Assert.That(tanks2.All(t => t.ManufacturerId == manufacturer2.Id), Is.True);
    }

    [Test]
    public async Task InsertManufacturerAsync_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var manufacturer = new Manufacturer("Company & Co.", "123 Main St., Suite #100", true);

        // Act
        var id = await _dbService.InsertManufacturerAsync(manufacturer);

        // Assert
        using var cmd = new NpgsqlCommand(
            "SELECT Name, Address FROM Manufacturer WHERE Id = @id",
            _connection);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetString(0), Is.EqualTo("Company & Co."));
        Assert.That(reader.GetString(1), Is.EqualTo("123 Main St., Suite #100"));
    }

    [Test]
    public async Task GetTanksByManufacturerIdAsync_ShouldReturnCorrectTankTypes()
    {
        // Arrange
        var manufacturer = new Manufacturer("Test Company", "Test Address", true);
        await _dbService.InsertManufacturerAsync(manufacturer);

        var tank1 = new Tank("Model1", "SN1", TankType.Light, manufacturer.Id);
        var tank2 = new Tank("Model2", "SN2", TankType.Medium, manufacturer.Id);
        var tank3 = new Tank("Model3", "SN3", TankType.Heavy, manufacturer.Id);

        await _dbService.InsertTankAsync(tank1);
        await _dbService.InsertTankAsync(tank2);
        await _dbService.InsertTankAsync(tank3);

        // Act
        var tanks = await _dbService.GetTanksByManufacturerIdAsync(manufacturer.Id);

        // Assert
        Assert.That(tanks, Has.Count.EqualTo(3));
        Assert.That(tanks.Count(t => t.TankType == TankType.Light), Is.EqualTo(1));
        Assert.That(tanks.Count(t => t.TankType == TankType.Medium), Is.EqualTo(1));
        Assert.That(tanks.Count(t => t.TankType == TankType.Heavy), Is.EqualTo(1));
    }
}
