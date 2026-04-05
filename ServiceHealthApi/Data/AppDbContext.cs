using Microsoft.EntityFrameworkCore;
using ServiceHealthApi.Models;

namespace ServiceHealthApi.Data;

/// <summary>
/// The EF Core DbContext is the bridge between your C# code and the database.
/// Think of it as a "session" with the database — you query and save through it.
///
/// DbSet properties map to database tables. EF Core handles creating the table schema
/// based on the properties of your model classes (ServiceEntry in this case).
/// </summary>
public class AppDbContext : DbContext
{
    // This constructor receives options (like the connection string) via Dependency Injection.
    // The options are configured in Program.cs when we call AddDbContext.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// This DbSet maps to a "Services" table in the database.
    /// You can query it with LINQ: _context.Services.Where(s => s.Status == ServiceStatus.Healthy)
    /// </summary>
    public DbSet<ServiceEntry> Services => Set<ServiceEntry>();

    /// <summary>
    /// OnModelCreating lets you configure the database schema beyond what EF infers from your models.
    /// Here we use it to seed initial data — EF will insert these rows when the database is created.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Convert the Status enum to a string in the database for readability.
        // Without this, EF stores enums as integers (0, 1, 2, 3) which are harder to debug.
        modelBuilder.Entity<ServiceEntry>()
            .Property(s => s.Status)
            .HasConversion<string>();
    }
}
