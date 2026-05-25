using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<HistoryItem> HistoryItems => Set<HistoryItem>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<CollectionModel> Collections => Set<CollectionModel>();
    public DbSet<CollectionRequestModel> CollectionRequests => Set<CollectionRequestModel>();
    public DbSet<EnvironmentModel> Environments => Set<EnvironmentModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistoryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Url);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.Request, r =>
            {
                r.ToJson();
                r.OwnsOne(req => req.Auth);
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
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectionModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
                r.OwnsOne(req => req.Auth);
                r.OwnsMany(req => req.Headers);
                r.OwnsMany(req => req.QueryParams);
                r.OwnsMany(req => req.FormData);
            });
        });

        modelBuilder.Entity<EnvironmentModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.Auth, a =>
            {
                a.ToJson();
            });

            entity.OwnsMany(e => e.Variables, v =>
            {
                v.ToJson();
            });
        });
    }
}
