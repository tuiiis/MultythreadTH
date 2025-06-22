using Microsoft.EntityFrameworkCore;
using EF.Models;

namespace EF.Services
{
    public class TankService
    {
        private readonly TankDbContext _context;
        private readonly Repository<Tank> _tankRepository;
        private readonly Repository<Manufacturer> _manufacturerRepository;

        public TankService(TankDbContext context)
        {
            _context = context;
            _tankRepository = new Repository<Tank>(context);
            _manufacturerRepository = new Repository<Manufacturer>(context);
        }

        /// <summary>
        /// Adds a new tank with a new manufacturer in a single transaction.
        /// </summary>
        /// <param name="manufacturerName">The name of the manufacturer.</param>
        /// <param name="manufacturerAddress">The address of the manufacturer.</param>
        /// <param name="isChildCompany">Indicates if the manufacturer is a child company.</param>
        /// <param name="tankModel">The model of the tank.</param>
        /// <param name="serialNumber">The serial number of the tank.</param>
        /// <param name="tankType">The type of the tank.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
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

        /// <summary>
        /// Gets all tanks associated with a specific manufacturer by manufacturer ID.
        /// </summary>
        /// <param name="manufacturerId">The unique identifier of the manufacturer.</param>
        /// <returns>A collection of tanks for the specified manufacturer.</returns>
        public async Task<IEnumerable<Tank>> GetTanksByManufacturerAsync(Guid manufacturerId)
        {
            return await _context.Tanks
                .Include(t => t.Manufacturer)
                .Where(t => t.ManufacturerId == manufacturerId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all tanks associated with manufacturers whose names contain the specified string.
        /// </summary>
        /// <param name="manufacturerName">The name or partial name of the manufacturer.</param>
        /// <returns>A collection of tanks for manufacturers matching the name.</returns>
        public async Task<IEnumerable<Tank>> GetTanksByManufacturerNameAsync(string manufacturerName)
        {
            return await _context.Tanks
                .Include(t => t.Manufacturer)
                .Where(t => t.Manufacturer != null && t.Manufacturer.Name.Contains(manufacturerName))
                .ToListAsync();
        }
    }
} 