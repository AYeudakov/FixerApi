﻿using System.Net;
using System.Text.Json;

namespace CurrencyExchange.Middlewares;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionMiddleware> _logger;

	public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
	{
		_logger = logger;
		_next = next;
	}

	public async Task InvokeAsync(HttpContext httpContext)
	{
		try
		{
			await _next(httpContext);
		}
		catch (Exception ex)
		{
			_logger.LogError("Something went wrong: {ex}", ex.Message);
			await HandleExceptionAsync(httpContext, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

		var message = exception switch
		{
			AccessViolationException => "Access violation error from the custom middleware",
			_ => "Internal Server Error from the custom middleware."
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(new
		{
			context.Response.StatusCode,
			Message = message
		}));
	}
}