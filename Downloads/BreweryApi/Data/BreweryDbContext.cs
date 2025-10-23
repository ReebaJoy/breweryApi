using Microsoft.EntityFrameworkCore;
using BreweryApi.Models;

namespace BreweryApi.Data;

public class BreweryDbContext : DbContext
{
    public BreweryDbContext(DbContextOptions<BreweryDbContext> options) : base(options)
    {
    }

    public DbSet<Brewery> Breweries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Brewery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.State);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.City).IsRequired();
        });
    }
}