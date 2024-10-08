﻿using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace med.common.library.configuration.service
{

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User != null ?
            _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) : string.Empty;

        public string Username => _httpContextAccessor.HttpContext?.User != null ? _httpContextAccessor.HttpContext?.User?.Identity?.Name : string.Empty;
    }
}
