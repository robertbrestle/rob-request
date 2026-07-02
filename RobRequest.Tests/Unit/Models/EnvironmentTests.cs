using RobRequest.Shared.Models.Auth;
using RobRequest.Shared.Models.Environments;
using Environment = RobRequest.Shared.Models.Environments.Environment;

namespace RobRequest.Tests.Unit.Models;

public class EnvironmentTests
{
    [Fact]
    public void Clone_ShouldCopyAllFields()
    {
        // Arrange
        var model = new Environment
        {
            Id = "env-1",
            Name = "Production",
            Description = "Prod env",
            SortOrder = 5,
            Variables = new List<EnvironmentVariable>
            {
                new() { Key = "API_URL", Value = "https://api.com", Enabled = true, IsSecret = false }
            },
            Auth = new AuthSettings
            {
                AuthType = AuthType.Bearer,
                AuthToken = "secret-token"
            },
            CreatedAt = DateTime.Now.AddDays(-1),
            UpdatedAt = DateTime.Now
        };

        // Act
        var clone = model.Clone();

        // Assert
        clone.Should().BeEquivalentTo(model);
        clone.Variables.Should().NotBeSameAs(model.Variables);
        clone.Variables[0].Should().NotBeSameAs(model.Variables[0]);
        clone.Auth.Should().NotBeSameAs(model.Auth);
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var model = new Environment();

        model.Name.Should().BeEmpty();
        model.Variables.Should().BeEmpty();
        model.Auth.Should().NotBeNull();
        model.Auth.AuthType.Should().Be(AuthType.None);
    }
}