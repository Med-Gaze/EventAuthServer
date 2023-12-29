using med.common.library.configuration.service;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace med.common.library.behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _identityService;

        public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = await Task.FromResult(typeof(TRequest).Name);

            _logger.LogInformation($"Request: {{Name}} {{@UserId}} {{@UserName}} {{@Request}}",
                requestName, _identityService.UserId, _identityService.Username, request);
        }
    }

}

