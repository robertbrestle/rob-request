using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HistoryItem> HistoryItems => Set<HistoryItem>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistoryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Url);

            entity.OwnsOne(e => e.Request, r =>
            {
                r.ToJson();
                r.OwnsMany(req => req.Headers);
                r.OwnsMany(req => req.QueryParams);
                r.OwnsMany(req => req.FormData);
            });
            entity.OwnsOne(e => e.Response, r =>
            {
                r.ToJson();
                r.OwnsMany(resp => resp.Headers);
            });
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
