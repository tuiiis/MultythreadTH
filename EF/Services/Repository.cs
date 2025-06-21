using Microsoft.EntityFrameworkCore;

namespace EF.Services
{
    /// <summary>
    /// Generic repository for CRUD operations on entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public class Repository<T> where T : class
    {
        private readonly TankDbContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public Repository(TankDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns>A list of all entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Gets an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The created entity.</returns>
        public async Task<T> CreateAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an existing entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Deletes an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The entity ID.</param>
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

    /// <summary>
    /// Static service for initializing the database with test data.
    /// </summary>
    public static class DBService
    {
        /// <summary>
        /// Initializes the database with manufacturers and tanks if empty.
        /// </summary>
        /// <param name="context">The database context.</param>
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