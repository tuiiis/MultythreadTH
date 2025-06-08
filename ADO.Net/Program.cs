using ADO.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

/// <summary>
/// The main program class that handles the application's entry point and configuration.
/// </summary>
class Program
{
    /// <summary>
    /// The main entry point of the application.
    /// Sets up dependency injection and runs the main application loop.
    /// </summary>
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var dbService = serviceProvider.GetRequiredService<DBService>();
        ////////////////////////////////////////////////////////////////////////

        dbService.CreateTables();

        while (true)
        {
            Console.WriteLine("1. Add Manufacturer");
            Console.WriteLine("2. Add Tank (for existing manufacturer)");
            Console.WriteLine("3. Fill database with 30 records");
            Console.WriteLine("4. Show tanks by manufacturer ID");
            Console.WriteLine("5. Exit");

            string? choice = Console.ReadLine();

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
                        await dbService.ShowTanksByManufacturerIdInteractiveAsync();
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
    }

    /// <summary>
    /// Configures the dependency injection services for the application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
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
}