using Microsoft.EntityFrameworkCore;
using BiteBot.Models;

namespace BiteBot.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants { get; set; } = null!;
    public DbSet<RestaurantAuditLog> RestaurantAuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.Url);
            
            // Add unique constraint on Name and City combination
            entity.HasIndex(e => new { e.Name, e.City })
                .IsUnique();

        modelBuilder.Entity<RestaurantAuditLog>(auditEntity =>
        {
            auditEntity.HasKey(e => e.Id);
            auditEntity.Property(e => e.RestaurantId).IsRequired();
            auditEntity.Property(e => e.Action).IsRequired().HasConversion<int>();
            auditEntity.Property(e => e.Timestamp).IsRequired();
            auditEntity.Property(e => e.Username).IsRequired();
            auditEntity.Property(e => e.UserId).IsRequired();
            auditEntity.Property(e => e.ChangeDetails);
            auditEntity.Property(e => e.ChangeDescription);
            
            // Index on RestaurantId for faster lookups
            auditEntity.HasIndex(e => e.RestaurantId);
            // Index on Timestamp for chronological queries
            auditEntity.HasIndex(e => e.Timestamp);
        });
        });
    }
}

