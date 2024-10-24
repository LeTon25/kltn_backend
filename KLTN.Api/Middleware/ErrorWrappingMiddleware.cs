using KLTN.Application.Helpers.Response;
using KLTN.Domain.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace KLTN.Api.Middleware
{
    public class ErrorWrappingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorWrappingMiddleware> _logger;

        public ErrorWrappingMiddleware(RequestDelegate next, ILogger<ErrorWrappingMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context,ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Xử lý các exception khác nhau
            if (ex is SplitScoreStructureException customEx)
            {
                context.Response.StatusCode = 400; 
            }
            else if (ex is InvalidScorePercentException customex)
            {
                context.Response.StatusCode = 400;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            // Trả về phản hồi chi tiết hơn tùy vào môi trường
            if (!context.Response.HasStarted && context.Response.StatusCode != 204)
            {
                context.Response.ContentType = "application/json";
                var response = new ApiResponse<string>(
                        context.Response.StatusCode,
                        ex.Message
                    );
                var json = JsonConvert.SerializeObject(response);
                await context.Response.WriteAsync(json);
            }
        }

    }
}
