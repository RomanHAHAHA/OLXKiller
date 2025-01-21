using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OLXKiller.API.Extensions;

public static class ControllerExtensions
{
    public static IActionResult CreateResponse(
        this ControllerBase controller,
        HttpStatusCode statusCode,
        string description)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => controller.NotFound(new { description }),
            HttpStatusCode.Unauthorized => controller.Unauthorized(new { description }),
            _ => controller.BadRequest(new { description }),
        };
    }
}
