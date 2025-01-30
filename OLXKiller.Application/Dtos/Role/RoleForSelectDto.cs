using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Dtos.Role;

public class RoleForSelectDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public RoleForSelectDto(RoleEntity roleEntity)
    {
        Id = roleEntity.Id;
        Name = roleEntity.Name;
    }
}
