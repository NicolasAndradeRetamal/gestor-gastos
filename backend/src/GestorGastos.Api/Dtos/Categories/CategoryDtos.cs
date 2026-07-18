namespace GestorGastos.Api.Dtos.Categories;

public record CategoryDto(Guid Id, string Name, string Color, string? Icon, bool IsDefault);

public record CategoryUpsertRequest(string Name, string Color, string? Icon);
