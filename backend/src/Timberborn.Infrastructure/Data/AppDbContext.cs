using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Models;

namespace Timberborn.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Adapter> Adapters => Set<Adapter>();
    public DbSet<Lever> Levers => Set<Lever>();
    public DbSet<AutomationProgram> Programs => Set<AutomationProgram>();
    public DbSet<AdapterLog> AdapterLogs => Set<AdapterLog>();
    public DbSet<ActionLog> ActionLogs => Set<ActionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adapter>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Slug).IsUnique();
            e.HasMany(a => a.Logs).WithOne(l => l.Adapter).HasForeignKey(l => l.AdapterId);
        });

        modelBuilder.Entity<Lever>(e => e.HasKey(l => l.Id));

        modelBuilder.Entity<AutomationProgram>(e => e.HasKey(p => p.Id));

        modelBuilder.Entity<AdapterLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.ReceivedAt);
        });

        modelBuilder.Entity<ActionLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Lever).WithMany().HasForeignKey(l => l.LeverId);
            e.HasIndex(l => l.CalledAt);
        });
    }
}
