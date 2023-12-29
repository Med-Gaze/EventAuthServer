using med.common.library.configuration.service;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace med.common.library.behaviours
{
    public class UnhandledExceptionBehaviour<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
   where TRequest : IRequest<TResult>
    {
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _identityService;
        public UnhandledExceptionBehaviour(ILogger<TRequest> logger, ICurrentUserService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }


        public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@UserId} {@Username} {@Request}", requestName, _identityService.UserId, _identityService.Username, request);

                throw;
            }
        }
    }

}