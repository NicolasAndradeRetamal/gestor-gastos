using FluentAssertions;
using GestorGastos.Domain.Entities;

namespace GestorGastos.Tests.Unit.Domain;

public class UserLockoutTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RegisterFailedLogin_BelowThreshold_DoesNotLock()
    {
        var user = new User();

        for (var i = 0; i < 4; i++)
            user.RegisterFailedLogin(Now);

        user.FailedLoginAttempts.Should().Be(4);
        user.IsLockedOut(Now).Should().BeFalse();
    }

    [Fact]
    public void RegisterFailedLogin_AtThreshold_LocksForFifteenMinutes()
    {
        var user = new User();

        for (var i = 0; i < 5; i++)
            user.RegisterFailedLogin(Now);

        user.IsLockedOut(Now).Should().BeTrue();
        user.LockedUntil.Should().Be(Now.AddMinutes(15));
        // Counter resets after locking so the next window starts clean.
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void IsLockedOut_AfterLockExpires_ReturnsFalse()
    {
        var user = new User();
        for (var i = 0; i < 5; i++)
            user.RegisterFailedLogin(Now);

        user.IsLockedOut(Now.AddMinutes(16)).Should().BeFalse();
    }

    [Fact]
    public void RegisterFailedLogin_AfterCountingWindowExpires_ResetsCounter()
    {
        var user = new User();
        for (var i = 0; i < 4; i++)
            user.RegisterFailedLogin(Now);

        // A stale attempt (>15 min later) restarts the count instead of adding on.
        user.RegisterFailedLogin(Now.AddMinutes(16));

        user.FailedLoginAttempts.Should().Be(1);
        user.IsLockedOut(Now.AddMinutes(16)).Should().BeFalse();
    }

    [Fact]
    public void RegisterSuccessfulLogin_ClearsLockoutState()
    {
        var user = new User();
        for (var i = 0; i < 5; i++)
            user.RegisterFailedLogin(Now);

        user.RegisterSuccessfulLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();
        user.LastFailedLoginAt.Should().BeNull();
    }
}
