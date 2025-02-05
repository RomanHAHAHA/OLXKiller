using Microsoft.AspNetCore.Mvc;
using OLXKiller.Domain.Abstractions.Models;
using System.Net;

namespace OLXKiller.API.Extensions;

public static class ControllerExtensions
{
    public static IActionResult HandleErrorResponse(
        this ControllerBase controller,
        IBaseResponse response)
    {
        return CreateResultObject(controller, response.Status, response.Description);
    }

    public static IActionResult HandleErrorResponse<T>(
        this ControllerBase controller,
        IBaseResponse<T> response)
    {
        return CreateResultObject(controller, response.Status, response.Description);
    }

    private static IActionResult CreateResultObject(
        this ControllerBase controller,
        HttpStatusCode httpStatusCode, 
        string description)
    {
        var jsonError = new { description };

        return httpStatusCode switch
        {
            HttpStatusCode.NotFound => controller.NotFound(jsonError),
            HttpStatusCode.Unauthorized => controller.Unauthorized(jsonError),
            HttpStatusCode.Conflict => controller.Conflict(jsonError),
            _ => controller.BadRequest(jsonError),
        };
    }
}
