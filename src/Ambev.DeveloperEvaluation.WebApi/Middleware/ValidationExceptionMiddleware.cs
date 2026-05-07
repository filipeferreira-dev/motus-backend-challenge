using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (DomainException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "Domain rule violated", new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = Array.Empty<ValidationErrorDetail>()
                });
            }
            catch (KeyNotFoundException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, "Not found", new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = Array.Empty<ValidationErrorDetail>()
                });
            }
            catch (InvalidOperationException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "Invalid operation", new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = Array.Empty<ValidationErrorDetail>()
                });
            }
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            return WriteResponseAsync(context, StatusCodes.Status400BadRequest, "Validation Failed", new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = exception.Errors.Select(error => (ValidationErrorDetail)error)
            });
        }

        private static Task WriteResponseAsync(HttpContext context, int statusCode, string _logCategory, ApiResponse response)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
