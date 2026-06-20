using Microsoft.AspNetCore.Identity;

namespace RobRequest.Tests.Unit.Services;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly UserService _sut;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _passwordHasher = new PasswordHasher<User>();
        _sut = new UserService(_db, _passwordHasher);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task ChangeGroupAsync_WithValidData_ReturnsTrueAndUpdatesGroup()
    {
        // Arrange
        var group1 = new UserGroup { Id = "group-1", Name = "user" };
        var group2 = new UserGroup { Id = "group-2", Name = "admin" };
        _db.UserGroups.AddRange(group1, group2);
        
        var user = new User
        {
            Id = "user-1",
            Username = "testuser",
            GroupId = "group-1"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ChangeGroupAsync("user-1", "group-2");

        // Assert
        result.Should().BeTrue();
        var updatedUser = await _db.Users.FindAsync("user-1");
        updatedUser!.GroupId.Should().Be("group-2");
    }

    [Fact]
    public async Task ChangeGroupAsync_WithInvalidUser_ReturnsFalse()
    {
        // Arrange
        var group = new UserGroup { Id = "group-1", Name = "user" };
        _db.UserGroups.Add(group);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ChangeGroupAsync("non-existent", "group-1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeGroupAsync_WithInvalidGroup_ReturnsFalse()
    {
        // Arrange
        var group = new UserGroup { Id = "group-1", Name = "user" };
        _db.UserGroups.Add(group);
        var user = new User
        {
            Id = "user-1",
            Username = "testuser",
            GroupId = "group-1"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ChangeGroupAsync("user-1", "non-existent-group");

        // Assert
        result.Should().BeFalse();
        var updatedUser = await _db.Users.FindAsync("user-1");
        updatedUser!.GroupId.Should().Be("group-1");
    }
}
