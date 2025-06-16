using Microsoft.EntityFrameworkCore;
using ADO.Net.Models;

namespace ADO.Net.Data
{
    public class TankDbContext : DbContext
    {
        public DbSet<Tank> Tanks { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }

        public TankDbContext(DbContextOptions<TankDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API configuration
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(m => m.Address)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(m => m.IsAChildCompany)
                    .IsRequired();
            });

            modelBuilder.Entity<Tank>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Model)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(t => t.SerialNumber)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(t => t.TankType)
                    .IsRequired()
                    .HasConversion<string>();

                entity.HasOne(t => t.Manufacturer)
                    .WithMany(m => m.Tanks)
                    .HasForeignKey(t => t.ManufacturerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
} 