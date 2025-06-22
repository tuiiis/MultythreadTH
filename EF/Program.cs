using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EF.Services;

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
int count = 30;

DBService.Initialize(context, count);

// Start console menu
var menu = new ConsoleMenu(context);
await menu.ShowMenuAsync();

Console.WriteLine("Application terminated. Press any key to exit...");
Console.ReadKey();