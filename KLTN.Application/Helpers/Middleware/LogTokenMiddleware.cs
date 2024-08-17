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
            
            if (authHeader.StartsWith("Bearer "))
            {
                var claims = context.User.Claims;
                foreach (var claim in claims)
                {
                    // In ra thông tin claim
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }
            }

            await _next(context);
        }

    }
}
