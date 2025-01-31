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

    public DbSet<Rating> Ratings { get; set; }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }

}