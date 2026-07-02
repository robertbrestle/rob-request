using RobRequest.Shared.Models.Users;

namespace RobRequest.Tests.Helpers;

public static class TestHelpers
{
    public const string TestUserId = "test-user";
    public const string TestGroupId = "test-group";

    public static CurrentUserService CreateTestCurrentUser()
    {
        return new CurrentUserService
        {
            UserId = TestUserId,
            Username = "testuser",
            GroupName = "user"
        };
    }

    public static SettingsService CreateTestSettingsService(AppDbContext db, int maxHistoryItems = 1000)
    {
        var currentUser = CreateTestCurrentUser();
        var service = new SettingsService(db, currentUser);

        var existing = db.UserSettings.FirstOrDefault(s => s.UserId == TestUserId);
        if (existing is null)
        {
            db.UserSettings.Add(new UserSettings { UserId = TestUserId, MaxHistoryItems = maxHistoryItems });
        }
        else
        {
            existing.MaxHistoryItems = maxHistoryItems;
        }
        db.SaveChanges();

        return service;
    }

    public static void SeedTestUser(AppDbContext db)
    {
        db.UserGroups.Add(new UserGroup
        {
            Id = TestGroupId,
            Name = "user"
        });
        db.Users.Add(new User
        {
            Id = TestUserId,
            Username = "testuser",
            PasswordHash = "not-a-real-hash",
            GroupId = TestGroupId,
            IsEnabled = true,
            IsApproved = true
        });
        db.SaveChanges();
    }
}