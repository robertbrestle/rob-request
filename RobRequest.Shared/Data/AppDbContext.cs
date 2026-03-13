using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HistoryItem> HistoryItems => Set<HistoryItem>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<CollectionModel> Collections => Set<CollectionModel>();
    public DbSet<CollectionRequestModel> CollectionRequests => Set<CollectionRequestModel>();

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

        modelBuilder.Entity<CollectionModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.Name);

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Requests)
                .WithOne(e => e.Collection)
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectionRequestModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CollectionId);

            entity.OwnsOne(e => e.Request, r =>
            {
                r.ToJson();
                r.OwnsMany(req => req.Headers);
                r.OwnsMany(req => req.QueryParams);
                r.OwnsMany(req => req.FormData);
            });
        });
    }
}
