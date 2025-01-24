using System.Data.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBarber.Models;

namespace MyBarber.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Barber> Barbers { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Barber>().HasData(
            new Barber { Id = 1, Name = "John Doe", Location= "Cair",Email = "test@gmail.com",HashPassword ="test",PhoneNumber = "123-456-7890", Pricing = 20, Services = "Haircut", isActive = true },
            new Barber { Id = 2, Name = "Jane Smith", Location= "Cair",Email = "test@gmail.com", HashPassword ="test", PhoneNumber = "123-456-7890", Pricing = 20, Services = "Haircut", isActive = true },
            new Barber { Id = 3, Name = "Mike Johnson", Location= "Cair",Email = "test@gmail.com",HashPassword ="test",PhoneNumber = "123-456-7890", Pricing = 20, Services = "Haircut", isActive = true }
        );
    }

}