using RobRequest.Shared.Data;
using RobRequest.Shared.Models;
using RobRequest.Shared.Services;

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
