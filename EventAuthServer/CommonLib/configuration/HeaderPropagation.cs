using med.common.library.constant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace med.common.library.configuration
{
    public  class HeaderPropagationMessangeHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HeaderPropagationMessangeHandlerBuilderFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                builder.AdditionalHandlers.Add(new HeaderPropagationMessangeHandler(_httpContextAccessor));
                next(builder);
            };
        }
    }
    public class HeaderPropagationMessangeHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HeaderPropagationMessangeHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
                var host = _httpContextAccessor.HttpContext.Request.Host.Host;
                if (!string.IsNullOrEmpty(host))
                {
                    var subDomain = host.Split('.').FirstOrDefault();

                    request.Headers.TryAddWithoutValidation(Common.RequestDomain, subDomain);
                }
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
                }
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
