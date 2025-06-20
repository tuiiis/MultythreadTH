using Microsoft.EntityFrameworkCore;
using EF.Models;

namespace EF.Services
{
    public class ConsoleMenu
    {
        private readonly DBService<Tank> _tankRepository;
        private readonly DBService<Manufacturer> _manufacturerRepository;
        private readonly TankService _tankService;
        private readonly TankDbContext _context;

        public ConsoleMenu(TankDbContext context)
        {
            _context = context;
            _tankRepository = new DBService<Tank>(context);
            _manufacturerRepository = new DBService<Manufacturer>(context);
            _tankService = new TankService(context);
        }

        public async Task ShowMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Tank Management System ===");
                Console.WriteLine("1. Manufacturer Operations");
                Console.WriteLine("2. Tank Operations");
                Console.WriteLine("3. Business Operations");
                Console.WriteLine("4. Exit");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ShowManufacturerMenuAsync();
                        break;
                    case "2":
                        await ShowTankMenuAsync();
                        break;
                    case "3":
                        await ShowBusinessMenuAsync();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task ShowManufacturerMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Manufacturer Operations ===");
                Console.WriteLine("1. View All Manufacturers");
                Console.WriteLine("2. View Manufacturer by ID");
                Console.WriteLine("3. Create Manufacturer");
                Console.WriteLine("4. Update Manufacturer");
                Console.WriteLine("5. Delete Manufacturer");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ViewAllManufacturersAsync();
                        break;
                    case "2":
                        await ViewManufacturerByIdAsync();
                        break;
                    case "3":
                        await CreateManufacturerAsync();
                        break;
                    case "4":
                        await UpdateManufacturerAsync();
                        break;
                    case "5":
                        await DeleteManufacturerAsync();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task ShowTankMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Tank Operations ===");
                Console.WriteLine("1. View All Tanks");
                Console.WriteLine("2. View Tank by ID");
                Console.WriteLine("3. Create Tank");
                Console.WriteLine("4. Update Tank");
                Console.WriteLine("5. Delete Tank");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ViewAllTanksAsync();
                        break;
                    case "2":
                        await ViewTankByIdAsync();
                        break;
                    case "3":
                        await CreateTankAsync();
                        break;
                    case "4":
                        await UpdateTankAsync();
                        break;
                    case "5":
                        await DeleteTankAsync();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task ShowBusinessMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Business Operations ===");
                Console.WriteLine("1. Add Tank with New Manufacturer");
                Console.WriteLine("2. Get Tanks by Manufacturer");
                Console.WriteLine("3. Back to Main Menu");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await AddTankWithNewManufacturerAsync();
                        break;
                    case "2":
                        await GetTanksByManufacturerAsync();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Manufacturer CRUD operations
        private async Task ViewAllManufacturersAsync()
        {
            Console.Clear();
            Console.WriteLine("=== All Manufacturers ===");
            var manufacturers = await _manufacturerRepository.GetAllAsync();
            foreach (var manufacturer in manufacturers)
            {
                Console.WriteLine($"ID: {manufacturer.Id}");
                Console.WriteLine($"Name: {manufacturer.Name}");
                Console.WriteLine($"Address: {manufacturer.Address}");
                Console.WriteLine($"Is Child Company: {manufacturer.IsAChildCompany}");
                Console.WriteLine(new string('-', 50));
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task ViewManufacturerByIdAsync()
        {
            Console.Write("Enter Manufacturer ID: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
                if (manufacturer != null)
                {
                    Console.WriteLine($"ID: {manufacturer.Id}");
                    Console.WriteLine($"Name: {manufacturer.Name}");
                    Console.WriteLine($"Address: {manufacturer.Address}");
                    Console.WriteLine($"Is Child Company: {manufacturer.IsAChildCompany}");
                }
                else
                {
                    Console.WriteLine("Manufacturer not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task CreateManufacturerAsync()
        {
            Console.Write("Enter Name: ");
            var name = Console.ReadLine() ?? "";
            Console.Write("Enter Address: ");
            var address = Console.ReadLine() ?? "";
            Console.Write("Is Child Company (y/n): ");
            var isChild = Console.ReadLine()?.ToLower() == "y";

            var manufacturer = new Manufacturer(name, address, isChild);
            await _manufacturerRepository.CreateAsync(manufacturer);
            Console.WriteLine("Manufacturer created successfully!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdateManufacturerAsync()
        {
            Console.Write("Enter Manufacturer ID to update: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
                if (manufacturer != null)
                {
                    Console.Write($"Enter Name (current: {manufacturer.Name}): ");
                    var name = Console.ReadLine();
                    if (!string.IsNullOrEmpty(name)) manufacturer.Name = name;

                    Console.Write($"Enter Address (current: {manufacturer.Address}): ");
                    var address = Console.ReadLine();
                    if (!string.IsNullOrEmpty(address)) manufacturer.Address = address;

                    Console.Write($"Is Child Company (current: {manufacturer.IsAChildCompany}) (y/n): ");
                    var isChildInput = Console.ReadLine()?.ToLower();
                    if (isChildInput == "y" || isChildInput == "n")
                        manufacturer.IsAChildCompany = isChildInput == "y";

                    await _manufacturerRepository.UpdateAsync(manufacturer);
                    Console.WriteLine("Manufacturer updated successfully!");
                }
                else
                {
                    Console.WriteLine("Manufacturer not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task DeleteManufacturerAsync()
        {
            Console.Write("Enter Manufacturer ID to delete: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                await _manufacturerRepository.DeleteAsync(id);
                Console.WriteLine("Manufacturer deleted successfully!");
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Tank CRUD operations
        private async Task ViewAllTanksAsync()
        {
            Console.Clear();
            Console.WriteLine("=== All Tanks ===");
            var tanks = await _context.Tanks.Include(t => t.Manufacturer).ToListAsync();
            foreach (var tank in tanks)
            {
                Console.WriteLine($"ID: {tank.Id}");
                Console.WriteLine($"Model: {tank.Model}");
                Console.WriteLine($"Serial Number: {tank.SerialNumber}");
                Console.WriteLine($"Type: {tank.TankType}");
                Console.WriteLine($"Manufacturer: {tank.Manufacturer?.Name ?? "Unknown"}");
                Console.WriteLine(new string('-', 50));
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task ViewTankByIdAsync()
        {
            Console.Write("Enter Tank ID: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                var tank = await _context.Tanks.Include(t => t.Manufacturer).FirstOrDefaultAsync(t => t.Id == id);
                if (tank != null)
                {
                    Console.WriteLine($"ID: {tank.Id}");
                    Console.WriteLine($"Model: {tank.Model}");
                    Console.WriteLine($"Serial Number: {tank.SerialNumber}");
                    Console.WriteLine($"Type: {tank.TankType}");
                    Console.WriteLine($"Manufacturer: {tank.Manufacturer?.Name ?? "Unknown"}");
                }
                else
                {
                    Console.WriteLine("Tank not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task CreateTankAsync()
        {
            Console.Write("Enter Model: ");
            var model = Console.ReadLine() ?? "";
            Console.Write("Enter Serial Number: ");
            var serialNumber = Console.ReadLine() ?? "";
            Console.WriteLine("Select Tank Type:");
            Console.WriteLine("1. Light");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. Heavy");
            Console.Write("Choice: ");

            TankType tankType = TankType.Light;
            if (int.TryParse(Console.ReadLine(), out var typeChoice))
            {
                tankType = typeChoice switch
                {
                    1 => TankType.Light,
                    2 => TankType.Medium,
                    3 => TankType.Heavy,
                    _ => TankType.Light
                };
            }

            // Show manufacturers
            var manufacturers = await _manufacturerRepository.GetAllAsync();
            Console.WriteLine("Available Manufacturers:");
            foreach (var m in manufacturers.Take(10))
            {
                Console.WriteLine($"{m.Id}: {m.Name}");
            }
            Console.Write("Enter Manufacturer ID: ");

            if (Guid.TryParse(Console.ReadLine(), out var manufacturerId))
            {
                var tank = new Tank(model, serialNumber, tankType, manufacturerId);
                await _tankRepository.CreateAsync(tank);
                Console.WriteLine("Tank created successfully!");
            }
            else
            {
                Console.WriteLine("Invalid Manufacturer ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdateTankAsync()
        {
            Console.Write("Enter Tank ID to update: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                var tank = await _tankRepository.GetByIdAsync(id);
                if (tank != null)
                {
                    Console.Write($"Enter Model (current: {tank.Model}): ");
                    var model = Console.ReadLine();
                    if (!string.IsNullOrEmpty(model)) tank.Model = model;

                    Console.Write($"Enter Serial Number (current: {tank.SerialNumber}): ");
                    var serialNumber = Console.ReadLine();
                    if (!string.IsNullOrEmpty(serialNumber)) tank.SerialNumber = serialNumber;

                    await _tankRepository.UpdateAsync(tank);
                    Console.WriteLine("Tank updated successfully!");
                }
                else
                {
                    Console.WriteLine("Tank not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task DeleteTankAsync()
        {
            Console.Write("Enter Tank ID to delete: ");
            if (Guid.TryParse(Console.ReadLine(), out var id))
            {
                await _tankRepository.DeleteAsync(id);
                Console.WriteLine("Tank deleted successfully!");
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Business operations
        private async Task AddTankWithNewManufacturerAsync()
        {
            Console.WriteLine("=== Add Tank with New Manufacturer ===");
            Console.Write("Enter Manufacturer Name: ");
            var manufacturerName = Console.ReadLine() ?? "";
            Console.Write("Enter Manufacturer Address: ");
            var manufacturerAddress = Console.ReadLine() ?? "";
            Console.Write("Is Child Company (y/n): ");
            var isChild = Console.ReadLine()?.ToLower() == "y";

            Console.Write("Enter Tank Model: ");
            var tankModel = Console.ReadLine() ?? "";
            Console.Write("Enter Tank Serial Number: ");
            var serialNumber = Console.ReadLine() ?? "";
            Console.WriteLine("Select Tank Type:");
            Console.WriteLine("1. Light");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. Heavy");
            Console.Write("Choice: ");

            TankType tankType = TankType.Light;
            if (int.TryParse(Console.ReadLine(), out var typeChoice))
            {
                tankType = typeChoice switch
                {
                    1 => TankType.Light,
                    2 => TankType.Medium,
                    3 => TankType.Heavy,
                    _ => TankType.Light
                };
            }

            var success = await _tankService.AddTankWithNewManufacturerAsync(
                manufacturerName, manufacturerAddress, isChild,
                tankModel, serialNumber, tankType);

            if (success)
            {
                Console.WriteLine("Tank and Manufacturer created successfully!");
            }
            else
            {
                Console.WriteLine("Failed to create Tank and Manufacturer. Transaction was rolled back.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task GetTanksByManufacturerAsync()
        {
            Console.Write("Enter Manufacturer Name (partial match): ");
            var manufacturerName = Console.ReadLine() ?? "";

            var tanks = await _tankService.GetTanksByManufacturerNameAsync(manufacturerName);

            Console.WriteLine($"=== Tanks for Manufacturer containing '{manufacturerName}' ===");
            foreach (var tank in tanks)
            {
                Console.WriteLine($"Tank ID: {tank.Id}");
                Console.WriteLine($"Model: {tank.Model}");
                Console.WriteLine($"Serial Number: {tank.SerialNumber}");
                Console.WriteLine($"Type: {tank.TankType}");
                Console.WriteLine($"Manufacturer: {tank.Manufacturer?.Name}");
                Console.WriteLine(new string('-', 40));
            }

            if (!tanks.Any())
            {
                Console.WriteLine("No tanks found for the specified manufacturer.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
} 