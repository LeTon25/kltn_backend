using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Middleware
{
    public class LogTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public LogTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            
            Console.WriteLine("KKKKK");
            Console.WriteLine(authHeader);  
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                // In token ra console hoặc log
                Console.WriteLine($"Token: {token}");
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

    }
}
