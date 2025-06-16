using ADO.Net.Data;
using ADO.Net.Models;
using EF.Data;

namespace ADO.Net.Data
{
    public static class DataInitializer
    {
        public static void Initialize(TankDbContext context)
        {
            if (context.Manufacturers.Any() || context.Tanks.Any())
                return;

            // Generate 30 manufacturers using DataFaker
            var manufacturers = DataFaker.CreateManufacturers(30);
            context.Manufacturers.AddRange(manufacturers);
            context.SaveChanges();

            // Generate 30 tanks using DataFaker
            var tanks = DataFaker.CreateTanks(manufacturers, 30);
            context.Tanks.AddRange(tanks);
            context.SaveChanges();
        }
    }
} 