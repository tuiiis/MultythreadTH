using Microsoft.EntityFrameworkCore;
using EF.Models;

namespace EF.Services
{
    public class TankService
    {
        private readonly TankDbContext _context;
        private readonly DBService<Tank> _tankRepository;
        private readonly DBService<Manufacturer> _manufacturerRepository;

        public TankService(TankDbContext context)
        {
            _context = context;
            _tankRepository = new DBService<Tank>(context);
            _manufacturerRepository = new DBService<Manufacturer>(context);
        }

        public async Task<bool> AddTankWithNewManufacturerAsync(
            string manufacturerName, string manufacturerAddress, bool isChildCompany,
            string tankModel, string serialNumber, TankType tankType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create new manufacturer
                var manufacturer = new Manufacturer(manufacturerName, manufacturerAddress, isChildCompany);
                await _manufacturerRepository.CreateAsync(manufacturer);

                // Create new tank
                var tank = new Tank(tankModel, serialNumber, tankType, manufacturer.Id);
                await _tankRepository.CreateAsync(tank);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Transaction failed: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Tank>> GetTanksByManufacturerAsync(Guid manufacturerId)
        {
            return await _context.Tanks
                .Include(t => t.Manufacturer)
                .Where(t => t.ManufacturerId == manufacturerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tank>> GetTanksByManufacturerNameAsync(string manufacturerName)
        {
            return await _context.Tanks
                .Include(t => t.Manufacturer)
                .Where(t => t.Manufacturer != null && t.Manufacturer.Name.Contains(manufacturerName))
                .ToListAsync();
        }
    }
} 