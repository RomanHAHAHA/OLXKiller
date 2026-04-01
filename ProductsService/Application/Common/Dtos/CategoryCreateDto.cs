namespace ProductsService.Application.Common.Dtos;

public record CategoryCreateDto(Guid? ParentCategoryId, string Name);