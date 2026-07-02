using Microsoft.AspNetCore.Identity;
using RobRequest.Shared.Models.Users;

namespace RobRequest.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _sut;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _passwordHasher = new PasswordHasher<User>();
        _sut = new AuthService(_db, _passwordHasher);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ReturnsTrueAndUpdatesPassword()
    {
        // Arrange
        var group = new UserGroup { Id = "group-1", Name = "user" };
        _db.UserGroups.Add(group);
        var user = new User
        {
            Id = "user-1",
            Username = "testuser",
            PasswordHash = "old-hash",
            GroupId = "group-1"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ChangePasswordAsync("user-1", "new-password-123");

        // Assert
        result.Should().BeTrue();
        var updatedUser = await _db.Users.FindAsync("user-1");
        updatedUser!.PasswordHash.Should().NotBe("old-hash");
        _passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, "new-password-123")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithTooShortPassword_ReturnsFalse()
    {
        // Arrange
        var group = new UserGroup { Id = "group-1", Name = "user" };
        _db.UserGroups.Add(group);
        var user = new User
        {
            Id = "user-1",
            Username = "testuser",
            PasswordHash = "old-hash",
            GroupId = "group-1"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ChangePasswordAsync("user-1", "short");

        // Assert
        result.Should().BeFalse();
        var updatedUser = await _db.Users.FindAsync("user-1");
        updatedUser!.PasswordHash.Should().Be("old-hash");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Act
        var result = await _sut.ChangePasswordAsync("non-existent", "new-password-123");

        // Assert
        result.Should().BeFalse();
    }
}