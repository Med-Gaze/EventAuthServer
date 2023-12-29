using med.common.library.configuration.service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace med.common.library.middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly ICurrentUserService _identityService;
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger, ICurrentUserService identityService)
        {
            _next = next;
            _logger = logger;
            _identityService = identityService;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context?.Request;
            var response = context?.Response;

            _logger.LogInformation("Before Request: Request {@Name} {@UserId} {@Username}", request.Path, _identityService.UserId, _identityService.Username);
            
            await _next(context);

            _logger.LogInformation("After Request: Response {@Name} {@UserId} {@Username}", response.StatusCode, _identityService.UserId, _identityService.Username);

        }
    }
}
