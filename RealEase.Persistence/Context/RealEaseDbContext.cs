using Microsoft.EntityFrameworkCore;
using RealEase.Domain.Entities;

namespace RealEase.Persistence.Context
{
    public class RealEaseDbContext : DbContext
    {
        public RealEaseDbContext()
        {
        }

        public RealEaseDbContext(DbContextOptions<RealEaseDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito de nombres de tablas en minúsculas para PostgreSQL.
            // UseSnakeCaseNamingConvention (configurado en Program.cs) maneja las columnas.
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Property>().ToTable("properties");
            modelBuilder.Entity<Visit>().ToTable("visits");
            modelBuilder.Entity<Contract>().ToTable("contracts");
            modelBuilder.Entity<Payment>().ToTable("payments");
        }
    }
}
