using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ADO.Net.Data;
using ADO.Net.UI;

namespace ADO.Net
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure DbContext
            var options = new DbContextOptionsBuilder<TankDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using var context = new TankDbContext(options);

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Initialize data
            DataInitializer.Initialize(context);

            // Start console menu
            var menu = new ConsoleMenu(context);
            await menu.ShowMenuAsync();

            Console.WriteLine("Application terminated. Press any key to exit...");
            Console.ReadKey();
        }
    }
} 