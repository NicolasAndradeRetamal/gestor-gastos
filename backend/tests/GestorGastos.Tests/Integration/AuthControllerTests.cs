using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Tests.Integration.Fixtures;

namespace GestorGastos.Tests.Integration;

public class AuthControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Register_WithValidData_Returns201WithToken()
    {
        var request = new RegisterRequest("ana@example.com", "Password123", "Ana");

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Email.Should().Be("ana@example.com");
        body.User.DisplayName.Should().Be("Ana");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400WithFieldErrors()
    {
        var request = new RegisterRequest("not-an-email", "Password123", "Ana");

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().ContainKey("errors");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var request = new RegisterRequest("dup@example.com", "Password123", "Ana");
        (await Client.PostAsJsonAsync("/api/auth/register", request)).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/register", request with { DisplayName = "Otro" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var register = new RegisterRequest("login@example.com", "Password123", "Ana");
        (await Client.PostAsJsonAsync("/api/auth/register", register)).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@example.com", "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401WithGenericMessage()
    {
        var register = new RegisterRequest("wrongpass@example.com", "Password123", "Ana");
        (await Client.PostAsJsonAsync("/api/auth/register", register)).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("wrongpass@example.com", "WrongPassword"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@example.com", "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var (authedClient, user) = await CreateAuthenticatedClientAsync("me@example.com");

        var response = await authedClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body!.Id.Should().Be(user.Id);
        body.Email.Should().Be("me@example.com");
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await Client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
