using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OLXKiller.API.Middlewares;

public class RequestLoggingMiddleware(
    ILogger<RequestLoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.EnableBuffering(); 
        context.Request.Body.Position = 0;

        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            logger.LogInformation($"Request Body:\n{body}");
            context.Request.Body.Position = 0; 
        }

        await next(context);

        if (context.Response.StatusCode == 400 &&
            context.Items["ModelState"] is ModelStateDictionary modelState)
        {
            foreach (var error in modelState)
            {
                logger.LogError($"Validation error for field {error.Key}: " +
                                 $"{string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }
    }
}
