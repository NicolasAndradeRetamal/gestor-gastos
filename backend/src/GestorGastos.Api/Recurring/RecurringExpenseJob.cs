using GestorGastos.Infrastructure.Recurring;
using Quartz;

namespace GestorGastos.Api.Recurring;

/// <summary>Hourly Quartz job that materializes due recurring-expense templates into real expenses.</summary>
[DisallowConcurrentExecution]
public class RecurringExpenseJob(IRecurringExpenseGenerator generator, ILogger<RecurringExpenseJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = await generator.GenerateDueAsync(today, context.CancellationToken);
        if (created > 0)
            logger.LogInformation("Generated {Count} recurring expense(s) for {Date}", created, today);
    }
}
