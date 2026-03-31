using System.Net;
using System.Text.Json;
using NeedApp.Application.Common.Exceptions;
using NeedApp.Domain.Exceptions;

namespace NeedApp.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation failed.", ve.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, (IDictionary<string, string[]>?)null),
            UnauthorizedException ue => (HttpStatusCode.Forbidden, ue.Message, null),
            DomainException de => (HttpStatusCode.BadRequest, de.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
