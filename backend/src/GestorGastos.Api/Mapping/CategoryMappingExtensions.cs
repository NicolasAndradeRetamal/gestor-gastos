using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Domain.Entities;

namespace GestorGastos.Api.Mapping;

public static class CategoryMappingExtensions
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Color, category.Icon, category.IsDefault);
}
