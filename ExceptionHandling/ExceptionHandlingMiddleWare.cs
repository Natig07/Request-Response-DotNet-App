using System.Net;
using System.Text.Json;
using Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode status = HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred. Please try again later.";

        switch (exception)
        {
            case NotFoundException:
                status = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case ValidationExceptionC:
                status = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedException:
                status = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case ForbiddenException:
                status = HttpStatusCode.Forbidden;
                message = exception.Message;
                break;
            case ConflictException:
                status = HttpStatusCode.Conflict;
                message = exception.Message;
                break;
        }

        context.Response.StatusCode = (int)status;

        var response = new
        {
            success = false,
            error = message,
            traceId = context.TraceIdentifier,
            details = _env.IsDevelopment() ? exception.ToString() : null
        };

        return context.Response.WriteAsJsonAsync(response);
    }



}
