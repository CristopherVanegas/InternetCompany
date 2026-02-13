using InternetCompany.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace InternetCompany.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (Exception)
        {
            await HandleGenericExceptionAsync(context);
        }
    }

    private static async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = ex.Message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private static async Task HandleUnauthorizedExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = ex.Message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Ocurrió un error interno en el servidor."
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
