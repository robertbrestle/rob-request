namespace RobRequest.Tests.Unit.Services;

public class EnvironmentServiceTests
{
    private readonly EnvironmentService _sut = new();

    [Fact]
    public async Task Constructor_CreatesDefaultEnvironment()
    {
        var environments = await _sut.GetEnvironmentsAsync();

        environments.Should().HaveCount(1);
        environments[0].Name.Should().Be("Default");
    }

    [Fact]
    public void ActiveEnvironmentId_ShouldBeSetToDefault()
    {
        _sut.ActiveEnvironmentId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SubstituteVariablesAsync_ReplacesVariables()
    {
        var env = await _sut.GetActiveEnvironmentAsync();
        env.Should().NotBeNull();

        await _sut.SetVariableAsync(env!.Id, "baseUrl", "https://api.example.com");

        var result = await _sut.SubstituteVariablesAsync("{{baseUrl}}/users");

        result.Should().Be("https://api.example.com/users");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_LeavesUnknownVariablesIntact()
    {
        var result = await _sut.SubstituteVariablesAsync("{{unknown}}/path");

        result.Should().Be("{{unknown}}/path");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_ReturnsInputWhenEmpty()
    {
        var result = await _sut.SubstituteVariablesAsync(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddEnvironmentAsync_AddsEnvironment()
    {
        var env = new EnvironmentModel { Name = "Production" };

        await _sut.AddEnvironmentAsync(env);
        var environments = await _sut.GetEnvironmentsAsync();

        environments.Should().HaveCount(2);
        environments.Should().Contain(e => e.Name == "Production");
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_RemovesEnvironment()
    {
        var env = new EnvironmentModel { Name = "ToDelete" };
        await _sut.AddEnvironmentAsync(env);

        await _sut.DeleteEnvironmentAsync(env.Id);
        var environments = await _sut.GetEnvironmentsAsync();

        environments.Should().NotContain(e => e.Id == env.Id);
    }

    [Fact]
    public async Task SetVariableAsync_UpdatesExistingVariable()
    {
        var env = await _sut.GetActiveEnvironmentAsync();
        await _sut.SetVariableAsync(env!.Id, "key", "value1");
        await _sut.SetVariableAsync(env.Id, "key", "value2");

        var updated = await _sut.GetEnvironmentAsync(env.Id);
        updated!.Variables.Should().ContainSingle(v => v.Key == "key")
            .Which.Value.Should().Be("value2");
    }
}
