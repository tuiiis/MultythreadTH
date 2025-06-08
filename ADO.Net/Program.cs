using ADO.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

/////////////////////////////////////////////////////////////////////

var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection, configuration);
var serviceProvider = serviceCollection.BuildServiceProvider();

var dbService = serviceProvider.GetRequiredService<DBService>();
////////////////////////////////////////////////////////////////////////
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton(configuration);
    services.AddTransient<NpgsqlConnection>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var connectionString = config.GetConnectionString("MyDatabase");
        return new NpgsqlConnection(connectionString);
    });
    services.AddTransient<DBService>();
}
//////////////////////////////////////////////////////////////////////

dbService.CreateTables();

while (true)
{
    Console.WriteLine("1. Add Manufacturer");
    Console.WriteLine("2. Add Tank (for existing manufacturer)");
    Console.WriteLine("3. Fill database with 30 records");
    Console.WriteLine("4. Show tanks by manufacturer ID");
    Console.WriteLine("5. Exit");

    string choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await dbService.AddManufacturerAsync();
                break;

            case "2":
                await dbService.AddTankAsync();
                break;

            case "3":
                await dbService.DataAdder();
                break;

            case "4":
                Console.WriteLine("Enter manufacturer ID (GUID):");
                string manufacturerIdStr = Console.ReadLine() ?? throw new ArgumentNullException("Manufacturer ID cannot be null");
                
                if (!Guid.TryParse(manufacturerIdStr, out Guid manufacturerId))
                {
                    Console.WriteLine("Invalid GUID format. Please enter a valid GUID.");
                    break;
                }

                var tanks = await dbService.GetTanksByManufacturerIdAsync(manufacturerId);
                if (tanks.Count == 0)
                {
                    Console.WriteLine("No tanks found for this manufacturer.");
                    break;
                }

                Console.WriteLine("\nTanks for manufacturer:");
                foreach (var tank in tanks)
                {
                    Console.WriteLine($"\nTank ID: {tank.Id}");
                    Console.WriteLine($"Model: {tank.Model}");
                    Console.WriteLine($"Serial Number: {tank.SerialNumber}");
                    Console.WriteLine($"Type: {tank.TankType}");
                    Console.WriteLine($"Manufacturer: {tank.Manufacturer.Name}");
                    Console.WriteLine($"Manufacturer Address: {tank.Manufacturer.Address}");
                    Console.WriteLine($"Is Child Company: {tank.Manufacturer.IsAChildCompany}");
                    Console.WriteLine("----------------------------------------");
                }
                break;

            case "5":
                Console.WriteLine("Exiting...");
                return;

            default:
                Console.WriteLine("Invalid input. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}