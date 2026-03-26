using Cafe1316.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Cafe1316.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);
        var response = context.Response;
        response.ContentType = "application/json";
        var errorResponse = new ErrorResponse();
        switch (exception)
        {
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = exception.Message;
                errorResponse.StatusCode = response.StatusCode;
                break;
            case BadRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                errorResponse.StatusCode = response.StatusCode;
                break;
            case UnauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = exception.Message;
                errorResponse.StatusCode = response.StatusCode;
                break;
            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = exception.Message;
                errorResponse.StatusCode = response.StatusCode;
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An internal server error occurred.";
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Details = exception.Message;
                break;
        }
        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await response.WriteAsync(result);
    }
}
/// <summary>
/// 错误响应模型
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}