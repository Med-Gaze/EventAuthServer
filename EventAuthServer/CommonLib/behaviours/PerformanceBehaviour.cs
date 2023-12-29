using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using med.common.library.configuration;
using med.common.library.configuration.service;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace med.common.library.behaviours
{


    public class PerformanceBehaviour<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
   where TRequest : IRequest<TResult>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _identityService;
        private readonly ModuleSettings _settings;
        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUserService identityService,
            IOptions<ModuleSettings> settings)
        {
            _timer = new Stopwatch();
            _settings = settings.Value;
            _logger = logger;
            _identityService = identityService;
        }


        
        public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            var requestName = typeof(TRequest).Name;

            if (elapsedMilliseconds > _settings.MaxApiExecutionTimeInMs)
            {

                _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)  {@UserId}  {@UserName} {@Request}",
                    requestName, elapsedMilliseconds, _identityService.UserId, _identityService.Username, request);
            }

            _logger.LogInformation("Success Request: {Name} ({ElapsedMilliseconds} milliseconds)  {@UserId}  {@UserName} {@Request}",
                   requestName, elapsedMilliseconds, _identityService.UserId, _identityService.Username, request);

            return response;
        }
    }
}

