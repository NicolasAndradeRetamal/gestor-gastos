using FluentAssertions;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using GestorGastos.Infrastructure.Recurring;
using GestorGastos.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Tests.Integration;

public class RecurringExpenseGeneratorTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    private static async Task<Guid> GlobalCategoryIdAsync(AppDbContext db) =>
        await db.Categories.Where(c => c.UserId == null).Select(c => c.Id).FirstAsync();

    private async Task<Guid> SeedTemplateAsync(Guid userId, int dayOfMonth, DateOnly? lastRunOn)
    {
        await using var db = CreateDbContext();
        var template = new RecurringExpense
        {
            UserId = userId,
            CategoryId = await GlobalCategoryIdAsync(db),
            Amount = 9.99m,
            DayOfMonth = dayOfMonth,
            Note = "Streaming",
            LastRunOn = lastRunOn,
        };
        db.RecurringExpenses.Add(template);
        await db.SaveChangesAsync();
        return template.Id;
    }

    private async Task<int> RunAsync(DateOnly today)
    {
        await using var db = CreateDbContext();
        return await new RecurringExpenseGenerator(db).GenerateDueAsync(today, default);
    }

    private async Task<List<Expense>> ExpensesOfAsync(Guid userId)
    {
        await using var db = CreateDbContext();
        return await db.Expenses.Where(e => e.UserId == userId).ToListAsync();
    }

    [Fact]
    public async Task GeneratesExpense_OnTheDueDay()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 15, lastRunOn: null);

        var count = await RunAsync(new DateOnly(2026, 3, 15));

        count.Should().Be(1);
        var expenses = await ExpensesOfAsync(user.Id);
        expenses.Should().ContainSingle();
        expenses[0].Amount.Should().Be(9.99m);
        expenses[0].SpentAt.Should().Be(new DateOnly(2026, 3, 15));
    }

    [Fact]
    public async Task DoesNotGenerate_WhenAlreadyRanThisMonth()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 15, lastRunOn: new DateOnly(2026, 3, 10));

        var count = await RunAsync(new DateOnly(2026, 3, 15));

        count.Should().Be(0);
        (await ExpensesOfAsync(user.Id)).Should().BeEmpty();
    }

    [Fact]
    public async Task DoesNotGenerate_BeforeTheDueDay()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 20, lastRunOn: null);

        var count = await RunAsync(new DateOnly(2026, 3, 15));

        count.Should().Be(0);
    }

    [Fact]
    public async Task ClampsToLastDay_ForShortMonths()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 31, lastRunOn: null);

        // February 2026 has 28 days: day 31 clamps to the 28th.
        var count = await RunAsync(new DateOnly(2026, 2, 28));

        count.Should().Be(1);
        (await ExpensesOfAsync(user.Id))[0].SpentAt.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public async Task CatchesUp_WhenSchedulerMissedTheExactDay()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 5, lastRunOn: null);

        // Runs on the 15th but the template was due on the 5th and never generated.
        var count = await RunAsync(new DateOnly(2026, 3, 15));

        count.Should().Be(1);
        (await ExpensesOfAsync(user.Id))[0].SpentAt.Should().Be(new DateOnly(2026, 3, 5));
    }

    [Fact]
    public async Task GeneratesAgain_InANewMonth()
    {
        var (_, user) = await CreateAuthenticatedClientAsync();
        await SeedTemplateAsync(user.Id, dayOfMonth: 15, lastRunOn: new DateOnly(2026, 2, 15));

        var count = await RunAsync(new DateOnly(2026, 3, 15));

        count.Should().Be(1);
    }
}
