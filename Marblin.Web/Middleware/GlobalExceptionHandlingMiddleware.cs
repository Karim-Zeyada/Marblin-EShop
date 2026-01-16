using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var user = context.User?.Identity?.Name ?? "Anonymous";
                _logger.LogError(ex, "Unhandled exception on {Path} for user {User}: {Message}",
                    context.Request.Path, user, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Check if the request is an API call or expects JSON
            if (IsApiRequest(context))
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var problemDetails = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                };

                var json = JsonSerializer.Serialize(problemDetails);
                return context.Response.WriteAsync(json);
            }
            else
            {
                // For MVC requests, redirect to the error page
                context.Response.Redirect("/Home/Error");
                return Task.CompletedTask;
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Headers["Accept"].ToString().Contains("application/json") ||
                   context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
