using Common.Domain.Abstractions;

namespace ProductsService.Domain.Entities;

public class UserSnapshot : Entity<Guid>
{
    public string NickName { get; set; } = string.Empty;

    public string AvatarImageName { get; set; } = string.Empty;

    public double Rating { get; set; }

    public List<Product> Products { get; set; } = [];
}