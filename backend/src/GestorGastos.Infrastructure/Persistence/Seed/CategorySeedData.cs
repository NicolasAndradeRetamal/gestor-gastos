using GestorGastos.Domain.Entities;

namespace GestorGastos.Infrastructure.Persistence.Seed;

/// <summary>
/// Predefined global categories (user_id null, is_default true), seeded by the
/// initial migration. Colors match the categorical chart palette.
/// </summary>
public static class CategorySeedData
{
    private static readonly DateTimeOffset SeedTimestamp = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Category> GetDefaults() =>
    [
        Build(Guid.Parse("00000000-0000-0000-0001-000000000001"), "Comida", "#F97316"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000002"), "Transporte", "#3B82F6"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000003"), "Vivienda", "#8B5CF6"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000004"), "Salud", "#F43F5E"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000005"), "Ocio", "#EAB308"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000006"), "Servicios", "#06B6D4"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000007"), "Educación", "#10B981"),
        Build(Guid.Parse("00000000-0000-0000-0001-000000000008"), "Otros", "#71717A"),
    ];

    private static Category Build(Guid id, string name, string color) => new()
    {
        Id = id,
        UserId = null,
        Name = name,
        Color = color,
        Icon = null,
        IsDefault = true,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp,
        Active = true,
    };
}
