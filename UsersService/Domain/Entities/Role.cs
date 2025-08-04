using Common.Domain.Abstractions;

namespace UsersService.Domain.Entities;

public sealed class Role(int id, string name) : Enumeration<Role>(id, name)
{
    public static readonly Role User = new(1, nameof(User));

    public static readonly Role Admin = new(2, nameof(Admin));
    
    public ICollection<Permission> Permissions { get; set; } = [];

    public ICollection<User> Users { get; set; } = [];
}