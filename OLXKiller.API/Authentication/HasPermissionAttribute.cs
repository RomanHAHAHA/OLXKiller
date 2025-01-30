using Microsoft.AspNetCore.Authorization;
using OLXKiller.Domain.Enums;

namespace OLXKiller.API.Authentication;

public class HasPermissionAttribute(Permission permission) : 
    AuthorizeAttribute(policy: permission.ToString())
{
}
