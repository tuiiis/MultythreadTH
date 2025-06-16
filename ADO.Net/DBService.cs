using Npgsql;
using ADO.Net.Models;
namespace ADO.Net;

/// <summary>
/// Service class for handling database operations related to manufacturers and tanks.
/// </summary>
public class DBService
{
    private readonly NpgsqlConnection _connection;

    // Manufacturer field names
    private const string ManufacturerId = nameof(Manufacturer.Id);
    private const string ManufacturerName = nameof(Manufacturer.Name);
    private const string ManufacturerAddress = nameof(Manufacturer.Address);
    private const string ManufacturerIsAChildCompany = nameof(Manufacturer.IsAChildCompany);
    private const string ManufacturerTable = nameof(Manufacturer);

    // Tank field names
    private const string TankId = nameof(Tank.Id);
    private const string TankModel = nameof(Tank.Model);
    private const string TankSerialNumber = nameof(Tank.SerialNumber);
    private const string TankType = nameof(Tank.TankType);
    private const string TankManufacturerId = nameof(Tank.ManufacturerId);
    private const string TankTable = nameof(Tank);
    private const int DataRowsCount = 30;

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
        string createManufacturer = $@"
        CREATE TABLE IF NOT EXISTS {ManufacturerTable} (
        {ManufacturerId} UUID PRIMARY KEY,
        {ManufacturerName} VARCHAR(100) NOT NULL,
        {ManufacturerAddress} VARCHAR(200),
        {ManufacturerIsAChildCompany} BOOLEAN);";

        string createTank = $@"
        CREATE TABLE IF NOT EXISTS {TankTable} (
        {TankId} UUID PRIMARY KEY,
        {TankModel} VARCHAR(100) NOT NULL,
        {TankSerialNumber} VARCHAR(100),
        {TankType} INTEGER,
        {TankManufacturerId} UUID,
        FOREIGN KEY ({TankManufacturerId}) REFERENCES {ManufacturerTable}({ManufacturerId}));";

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

        string sql = $@"
            INSERT INTO {ManufacturerTable} ({ManufacturerId}, {ManufacturerName}, {ManufacturerAddress}, {ManufacturerIsAChildCompany})
            VALUES (@{ManufacturerId}, @{ManufacturerName}, @{ManufacturerAddress}, @{ManufacturerIsAChildCompany})
            RETURNING {ManufacturerId};";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@" + ManufacturerId, manufacturer.Id);
        cmd.Parameters.AddWithValue("@" + ManufacturerName, manufacturer.Name);
        cmd.Parameters.AddWithValue("@" + ManufacturerAddress, manufacturer.Address);
        cmd.Parameters.AddWithValue("@" + ManufacturerIsAChildCompany, manufacturer.IsAChildCompany);

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

        string sql = $@"
            INSERT INTO {TankTable} ({TankId}, {TankModel}, {TankSerialNumber}, {TankType}, {TankManufacturerId})
            VALUES (@{TankId}, @{TankModel}, @{TankSerialNumber}, @{TankType}, @{TankManufacturerId});";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@" + TankId, tank.Id);
        cmd.Parameters.AddWithValue("@" + TankModel, tank.Model);
        cmd.Parameters.AddWithValue("@" + TankSerialNumber, tank.SerialNumber);
        cmd.Parameters.AddWithValue("@" + TankType, (int)tank.TankType);
        cmd.Parameters.AddWithValue("@" + TankManufacturerId, tank.ManufacturerId);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Adds sample data to the database by creating 30 manufacturers and their associated tanks.
    /// </summary>
    public async Task DataAdder()
    {
        var manufacturers = ClassFaker.CreateManufacturers(DataRowsCount);
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
        Console.WriteLine($"Enter {ManufacturerName}:");
        string name = Console.ReadLine() ?? throw new ArgumentNullException("Name cannot be null");

        Console.WriteLine($"Enter {ManufacturerAddress}:");
        string address = Console.ReadLine() ?? throw new ArgumentNullException("Address cannot be null");

        Console.WriteLine("Is this a child company? (Y/N):");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "N";
        bool isChild = input.ToLower() == "y";

        var manufacturer = new Manufacturer(name, address, isChild);
        Guid id = await InsertManufacturerAsync(manufacturer);
        Console.WriteLine($"Manufacturer added with {ManufacturerId}: {id}");
        return id;
    }

    /// <summary>
    /// Interactively adds a new tank by prompting for user input.
    /// </summary>
    public async Task AddTankAsync()
    {
        Console.WriteLine($"Enter {TankModel}:");
        string model = Console.ReadLine() ?? throw new ArgumentNullException("Model cannot be null");

        Console.WriteLine($"Enter {TankSerialNumber}:");
        string serial = Console.ReadLine() ?? throw new ArgumentNullException("Serial number cannot be null");

        Console.WriteLine($"Enter Tank Type:");
        var tankTypes = Enum.GetValues(typeof(TankType));
        for (int i = 0; i < tankTypes.Length; i++)
        {
            Console.WriteLine($"{i} - {tankTypes.GetValue(i)}");
        }
        if (!int.TryParse(Console.ReadLine(), out int typeValue) || !Enum.IsDefined(typeof(TankType), typeValue))
        {
            throw new ArgumentException("Invalid tank type");
        }
        TankType type = (TankType)typeValue;

        Console.WriteLine($"Enter {TankManufacturerId}:");
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

        string query = $@"
            SELECT t.{TankId}, t.{TankModel}, t.{TankSerialNumber}, t.{TankType}, t.{TankManufacturerId},
                   m.{ManufacturerName}, m.{ManufacturerAddress}, m.{ManufacturerIsAChildCompany}
            FROM {TankTable} t
            JOIN {ManufacturerTable} m ON t.{TankManufacturerId} = m.{ManufacturerId}
            WHERE t.{TankManufacturerId} = @{TankManufacturerId};";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@" + TankManufacturerId, manufacturerId);

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
        Console.WriteLine($"Enter {ManufacturerId} ({typeof(Guid).Name}):");
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
            Console.WriteLine($"\n{TankId}: {tank.Id}");
            Console.WriteLine($"{TankModel}: {tank.Model}");
            Console.WriteLine($"{TankSerialNumber}: {tank.SerialNumber}");
            Console.WriteLine($"{TankType}: {tank.TankType}");
            Console.WriteLine($"{ManufacturerName}: {tank.Manufacturer!.Name}");
            Console.WriteLine($"{ManufacturerAddress}: {tank.Manufacturer.Address}");
            Console.WriteLine($"{ManufacturerIsAChildCompany}: {tank.Manufacturer.IsAChildCompany}");
            Console.WriteLine("----------------------------------------");
        }
    }
}
