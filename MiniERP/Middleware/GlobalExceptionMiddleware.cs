using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniERP.Application.Exceptions;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniERP.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
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
                _logger.LogError(ex, "An unhandled exception occurred during the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            string message;
            string[]? errors = null;

            switch (exception)
            {
                case BusinessValidationException bve:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Business validation failed.";
                    errors = new[] { bve.Message };
                    break;

                case ConcurrencyConflictException cce:
                    statusCode = HttpStatusCode.Conflict;
                    message = "Concurrency conflict occurred.";
                    errors = new[] { cce.Message };
                    break;

                case DuplicateResourceException dre:
                    statusCode = HttpStatusCode.Conflict;
                    message = "Duplicate resource conflict.";
                    errors = new[] { dre.Message };
                    break;

                case AccountDeactivatedException ade:
                    statusCode = HttpStatusCode.Forbidden;
                    message = ade.Message;
                    errors = new[] { ade.Message };
                    break;

                case NotFoundException nfe:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Resource not found.";
                    errors = new[] { nfe.Message };
                    break;

                case FluentValidation.ValidationException fve:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validation failed.";
                    errors = fve.Errors.Select(e => e.ErrorMessage).ToArray();
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
                    errors = _env.IsDevelopment() ? new[] { exception.ToString() } : new[] { "Internal server error." };
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message = message,
                data = (object?)null,
                errors = errors
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
