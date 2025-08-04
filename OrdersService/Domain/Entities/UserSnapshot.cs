namespace OrdersService.Domain.Entities;

public class UserSnapshot
{
    public Guid Id { get; set; }

    public string NickName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    public string AvatarPath { get; set; } = string.Empty;

    public List<Order> Orders { get; set; } = [];
}