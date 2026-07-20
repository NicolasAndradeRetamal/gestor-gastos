using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Tests.Integration.Fixtures;
using OtpNet;

namespace GestorGastos.Tests.Integration;

public class AuthSecurityTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    private record ChallengeDto(bool TwoFactorRequired, string TwoFactorToken);
    private record SetupDto(string Secret, string OtpauthUri);
    private record EnableDto(List<string> RecoveryCodes);
    private record RefreshDto(string Token, string RefreshToken);

    private async Task<AuthResponse> RegisterAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123", "Test"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    [Fact]
    public async Task Register_IncludesRefreshTokenAndTwoFactorFlag()
    {
        var auth = await RegisterAsync("refresh-shape@example.com");

        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshTokenExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        auth.User.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndInvalidatesTheOldOne()
    {
        var auth = await RegisterAsync("rotate@example.com");

        var rotated = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken));
        rotated.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await rotated.Content.ReadFromJsonAsync<RefreshDto>())!;
        body.RefreshToken.Should().NotBe(auth.RefreshToken);

        // Reusing the original (now revoked) token is theft: rejected...
        var reuse = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // ...and it revokes the whole family, so the rotated token no longer works either.
        var afterReuse = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(body.RefreshToken));
        afterReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var auth = await RegisterAsync("logout@example.com");

        var logout = await Client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(auth.RefreshToken));
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refresh = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken));
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksAccountEvenWithCorrectPassword()
    {
        await RegisterAsync("lockme@example.com");

        for (var i = 0; i < 5; i++)
            await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("lockme@example.com", "WrongPassword"));

        var locked = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("lockme@example.com", "Password123"));
        locked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TwoFactor_FullFlow_ChallengesLoginAndVerifiesWithTotp()
    {
        var auth = await RegisterAsync("twofactor@example.com");
        var authedClient = AuthorizedClient(auth.Token);

        var setup = (await (await authedClient.PostAsync("/api/auth/2fa/setup", null))
            .Content.ReadFromJsonAsync<SetupDto>())!;
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));

        var enable = await authedClient.PostAsJsonAsync("/api/auth/2fa/enable", new TwoFactorEnableRequest(totp.ComputeTotp()));
        enable.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await enable.Content.ReadFromJsonAsync<EnableDto>())!;
        codes.RecoveryCodes.Should().HaveCount(10);

        // Login now returns a challenge instead of tokens.
        var login = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("twofactor@example.com", "Password123"));
        var challenge = (await login.Content.ReadFromJsonAsync<ChallengeDto>())!;
        challenge.TwoFactorRequired.Should().BeTrue();
        challenge.TwoFactorToken.Should().NotBeNullOrWhiteSpace();

        var verify = await Client.PostAsJsonAsync(
            "/api/auth/2fa/verify",
            new TwoFactorVerifyRequest(challenge.TwoFactorToken, totp.ComputeTotp()));
        verify.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = (await verify.Content.ReadFromJsonAsync<AuthResponse>())!;
        session.Token.Should().NotBeNullOrWhiteSpace();
        session.User.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task TwoFactor_VerifyWithRecoveryCode_Succeeds()
    {
        var auth = await RegisterAsync("recovery@example.com");
        var authedClient = AuthorizedClient(auth.Token);

        var setup = (await (await authedClient.PostAsync("/api/auth/2fa/setup", null))
            .Content.ReadFromJsonAsync<SetupDto>())!;
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));
        var codes = (await (await authedClient.PostAsJsonAsync("/api/auth/2fa/enable", new TwoFactorEnableRequest(totp.ComputeTotp())))
            .Content.ReadFromJsonAsync<EnableDto>())!;

        var login = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("recovery@example.com", "Password123"));
        var challenge = (await login.Content.ReadFromJsonAsync<ChallengeDto>())!;

        var verify = await Client.PostAsJsonAsync(
            "/api/auth/2fa/verify",
            new TwoFactorVerifyRequest(challenge.TwoFactorToken, codes.RecoveryCodes[0]));
        verify.StatusCode.Should().Be(HttpStatusCode.OK);

        // A recovery code is single-use: presenting it again fails.
        var login2 = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("recovery@example.com", "Password123"));
        var challenge2 = (await login2.Content.ReadFromJsonAsync<ChallengeDto>())!;
        var reuse = await Client.PostAsJsonAsync(
            "/api/auth/2fa/verify",
            new TwoFactorVerifyRequest(challenge2.TwoFactorToken, codes.RecoveryCodes[0]));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TwoFactor_Disable_RestoresSingleFactorLogin()
    {
        var auth = await RegisterAsync("disable2fa@example.com");
        var authedClient = AuthorizedClient(auth.Token);

        var setup = (await (await authedClient.PostAsync("/api/auth/2fa/setup", null))
            .Content.ReadFromJsonAsync<SetupDto>())!;
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));
        await authedClient.PostAsJsonAsync("/api/auth/2fa/enable", new TwoFactorEnableRequest(totp.ComputeTotp()));

        var disable = await authedClient.PostAsJsonAsync(
            "/api/auth/2fa/disable",
            new TwoFactorDisableRequest("Password123", totp.ComputeTotp()));
        disable.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Login returns tokens directly again (no challenge).
        var login = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("disable2fa@example.com", "Password123"));
        var session = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        session.Token.Should().NotBeNullOrWhiteSpace();
        session.User.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Login_BeyondRateLimit_Returns429()
    {
        await RegisterAsync("ratelimit@example.com");

        HttpResponseMessage? last = null;
        for (var i = 0; i < 11; i++)
            last = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ratelimit@example.com", "WrongPassword"));

        last!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private HttpClient AuthorizedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
