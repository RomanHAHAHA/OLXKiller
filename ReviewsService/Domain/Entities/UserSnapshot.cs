namespace ReviewsService.Domain.Entities;

public class UserSnapshot
{
    public Guid Id { get; set; }

    public string NickName { get; set; } = string.Empty;

    public string AvatarPath { get; set; } = string.Empty;

    public List<Review> Reviews { get; set; } = [];
}