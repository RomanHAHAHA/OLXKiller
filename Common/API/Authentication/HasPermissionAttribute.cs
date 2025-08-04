using Common.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Common.API.Authentication;

public class HasPermissionAttribute(PermissionEnum permission) : 
    AuthorizeAttribute(policy: permission.ToString())
{
}