using Microsoft.EntityFrameworkCore;

namespace EF.Services
{
    public class DBService<T> where T : class
    {
        private readonly TankDbContext _context;
        private readonly DbSet<T> _dbSet;

        public DBService(TankDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    // Non-generic DBService for static initialization
    public static class DBService
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