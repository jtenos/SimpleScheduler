using Grpc.Core;
using System.Net;

namespace SimpleSchedulerBlazor.Server;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error calling {path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode switch
            {
                StatusCode.NotFound => (int)HttpStatusCode.NotFound,
                StatusCode.OutOfRange or StatusCode.InvalidArgument => (int)HttpStatusCode.BadRequest,
                StatusCode.PermissionDenied => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError,
            };
            await context.Response.WriteAsync(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
