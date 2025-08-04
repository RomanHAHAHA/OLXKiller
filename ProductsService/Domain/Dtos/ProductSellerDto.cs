using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Dtos;

public class ProductSellerDto(Product product)
{
    public Guid UserId { get; set; } = product.UserId;

    public string NickName { get; set; } = product.User!.NickName;

    public string AvatarImageName { get; set; } = product.User!.AvatarImageName;

    public double Rating { get; set; } = product.User!.Rating;

    public string RegisterDate { get; set; } = $"{product.User!.CreatedAt.ToLocalTime():dd.MM.yyyy}";
}