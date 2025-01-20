using System.Data.Common;
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

}