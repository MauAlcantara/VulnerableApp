using Microsoft.EntityFrameworkCore;
using System;
using VulnerableApp.Models;
namespace VulnerableApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Solucionar el Warning del Decimal (18 dígitos en total, 2 decimales)
            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasPrecision(18, 2);

            // 2. Solucionar el Error usando una fecha estática y fija
            var fixedDate = new DateTime(2026, 1, 1, 12, 0, 0);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin",
                    Email = "admin@test.com",
                    Balance = 1000m,
                    CreatedAt = fixedDate // Usar la fecha fija aquí
                },
                new User
                {
                    Id = 2,
                    Username = "user1",
                    Password = "123456",
                    Email = "user@test.com",
                    Balance = 500m,
                    CreatedAt = fixedDate // Y aquí
                },
                new User
                {
                    Id = 3,
                    Username = "user2",
                    Password = "password",
                    Email = "user2@test.com",
                    Balance = 750m,
                    CreatedAt = fixedDate // Y aquí
                }
            );
        }
    }
}