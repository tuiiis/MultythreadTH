using Npgsql;
using ADO.Net.Models;
namespace ADO.Net;

/// <summary>
/// Service class for handling database operations related to manufacturers and tanks.
/// </summary>
public class DBService
{
    private readonly NpgsqlConnection _connection;

    /// <summary>
    /// Initializes a new instance of the DBService class.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    public DBService(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Creates the necessary database tables if they don't exist.
    /// Creates Manufacturer and Tank tables with appropriate relationships.
    /// </summary>
    public void CreateTables()
    {
        string createManufacturer = @"
        CREATE TABLE IF NOT EXISTS Manufacturer (
        Id UUID PRIMARY KEY,
        Name VARCHAR(100) NOT NULL,
        Address VARCHAR(200),
        IsAChildCompany BOOLEAN);";

        string createTank = @"
        CREATE TABLE IF NOT EXISTS Tank (
        Id UUID PRIMARY KEY,
        Model VARCHAR(100) NOT NULL,
        SerialNumber VARCHAR(100),
        TankType INTEGER,
        ManufacturerId UUID,
        FOREIGN KEY (ManufacturerId) REFERENCES Manufacturer(Id));";

        if (_connection.State != System.Data.ConnectionState.Open)
        {
            _connection.Open();
        }

        using (var cmd1 = new NpgsqlCommand(createManufacturer, _connection))
        {
            cmd1.ExecuteNonQuery();
        }

        using (var cmd2 = new NpgsqlCommand(createTank, _connection))
        {
            cmd2.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Inserts a new manufacturer into the database.
    /// </summary>
    /// <param name="manufacturer">The manufacturer object to insert.</param>
    /// <returns>The ID of the inserted manufacturer.</returns>
    public async Task<Guid> InsertManufacturerAsync(Manufacturer manufacturer)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        string sql = @"
            INSERT INTO Manufacturer (Id, Name, Address, IsAChildCompany)
            VALUES (@id, @name, @address, @isChild)
            RETURNING Id;";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("id", manufacturer.Id);
        cmd.Parameters.AddWithValue("name", manufacturer.Name);
        cmd.Parameters.AddWithValue("address", manufacturer.Address);
        cmd.Parameters.AddWithValue("isChild", manufacturer.IsAChildCompany);

        return (Guid)await cmd.ExecuteScalarAsync();
    }

    /// <summary>
    /// Inserts a new tank into the database.
    /// </summary>
    /// <param name="tank">The tank object to insert.</param>
    public async Task InsertTankAsync(Tank tank)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        string sql = @"
            INSERT INTO Tank (Id, Model, SerialNumber, TankType, ManufacturerId)
            VALUES (@id, @model, @serial, @type, @manId);";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("id", tank.Id);
        cmd.Parameters.AddWithValue("model", tank.Model);
        cmd.Parameters.AddWithValue("serial", tank.SerialNumber);
        cmd.Parameters.AddWithValue("type", (int)tank.TankType);
        cmd.Parameters.AddWithValue("manId", tank.ManufacturerId);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Adds sample data to the database by creating 30 manufacturers and their associated tanks.
    /// </summary>
    public async Task DataAdder()
    {
        var manufacturers = ClassFaker.CreateManufacturers(30);
        foreach (var manufacturer in manufacturers)
        {
            await InsertManufacturerAsync(manufacturer);
            var tank = ClassFaker.CreateTank(manufacturer);
            await InsertTankAsync(tank);
        }

        Console.WriteLine("Data adding completed!");
    }

    /// <summary>
    /// Interactively adds a new manufacturer by prompting for user input.
    /// </summary>
    /// <returns>The ID of the newly created manufacturer.</returns>
    public async Task<Guid> AddManufacturerAsync()
    {
        Console.WriteLine("Enter manufacturer name:");
        string name = Console.ReadLine() ?? throw new ArgumentNullException("Name cannot be null");

        Console.WriteLine("Enter manufacturer address:");
        string address = Console.ReadLine() ?? throw new ArgumentNullException("Address cannot be null");

        Console.WriteLine("Is this a child company? (Y/N):");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "N";
        bool isChild = input.ToLower() == "y";

        var manufacturer = new Manufacturer(name, address, isChild);
        Guid id = await InsertManufacturerAsync(manufacturer);
        Console.WriteLine($"Manufacturer added with ID: {id}");
        return id;
    }

    /// <summary>
    /// Interactively adds a new tank by prompting for user input.
    /// </summary>
    public async Task AddTankAsync()
    {
        Console.WriteLine("Enter model:");
        string model = Console.ReadLine() ?? throw new ArgumentNullException("Model cannot be null");

        Console.WriteLine("Enter serial number:");
        string serial = Console.ReadLine() ?? throw new ArgumentNullException("Serial number cannot be null");

        Console.WriteLine("Enter tank type (0 - Light, 1 - Medium, 2 - Heavy):");
        if (!int.TryParse(Console.ReadLine(), out int typeValue) || !Enum.IsDefined(typeof(TankType), typeValue))
        {
            throw new ArgumentException("Invalid tank type");
        }
        TankType type = (TankType)typeValue;

        Console.WriteLine("Enter manufacturer ID:");
        if (!Guid.TryParse(Console.ReadLine(), out Guid manufacturerId))
        {
            throw new ArgumentException("Invalid manufacturer ID format");
        }

        var tank = new Tank(model, serial, type, manufacturerId);
        await InsertTankAsync(tank);
        Console.WriteLine("Tank added successfully.");
    }

    /// <summary>
    /// Retrieves all tanks associated with a specific manufacturer.
    /// </summary>
    /// <param name="manufacturerId">The ID of the manufacturer to find tanks for.</param>
    /// <returns>A list of tanks associated with the specified manufacturer.</returns>
    public async Task<List<Tank>> GetTanksByManufacturerIdAsync(Guid manufacturerId)
    {
        var tanks = new List<Tank>();

        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        string query = @"
            SELECT t.Id, t.Model, t.SerialNumber, t.TankType, t.ManufacturerId,
                   m.Name, m.Address, m.IsAChildCompany
            FROM Tank t
            JOIN Manufacturer m ON t.ManufacturerId = m.Id
            WHERE t.ManufacturerId = @manId;";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("manId", manufacturerId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tank = new Tank
            {
                Id = reader.GetGuid(0),
                Model = reader.GetString(1),
                SerialNumber = reader.GetString(2),
                TankType = (TankType)reader.GetInt32(3),
                ManufacturerId = reader.GetGuid(4),
                Manufacturer = new Manufacturer()
                {
                    Id = reader.GetGuid(4),
                    Name = reader.GetString(5),
                    Address = reader.GetString(6),
                    IsAChildCompany = reader.GetBoolean(7)
                }
            };
            tanks.Add(tank);
        }

        return tanks;
    }

    /// <summary>
    /// Interactively displays all tanks for a specified manufacturer ID.
    /// Prompts the user for a manufacturer ID and displays the associated tanks.
    /// </summary>
    public async Task ShowTanksByManufacturerIdInteractiveAsync()
    {
        Console.WriteLine("Enter manufacturer ID (GUID):");
        string manufacturerIdStr = Console.ReadLine() ?? throw new ArgumentNullException("Manufacturer ID cannot be null");

        if (!Guid.TryParse(manufacturerIdStr, out Guid manufacturerId))
        {
            Console.WriteLine("Invalid GUID format. Please enter a valid GUID.");
            return;
        }

        var tanks = await GetTanksByManufacturerIdAsync(manufacturerId);
        if (tanks.Count == 0)
        {
            Console.WriteLine("No tanks found for this manufacturer.");
            return;
        }

        Console.WriteLine("\nTanks for manufacturer:");
        foreach (var tank in tanks)
        {
            Console.WriteLine($"\nTank ID: {tank.Id}");
            Console.WriteLine($"Model: {tank.Model}");
            Console.WriteLine($"Serial Number: {tank.SerialNumber}");
            Console.WriteLine($"Type: {tank.TankType}");
            Console.WriteLine($"Manufacturer: {tank.Manufacturer!.Name}");
            Console.WriteLine($"Manufacturer Address: {tank.Manufacturer.Address}");
            Console.WriteLine($"Is Child Company: {tank.Manufacturer.IsAChildCompany}");
            Console.WriteLine("----------------------------------------");
        }
    }
}
