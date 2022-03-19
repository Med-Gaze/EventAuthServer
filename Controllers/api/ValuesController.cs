using med.common.library.controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IdentityServer4.IdentityServerConstants;

namespace EventAuthServer.Controllers.api
{
    [Authorize(LocalApi.PolicyName)]
    public class ValuesController : BaseController
    {
        private readonly ILogger<ValuesController> logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ValuesController(ILogger<ValuesController> logger)
        {
            this.logger = logger;
        }
        /// <summary>
        /// Return data without authorization
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/v1/values/index
        ///     
        /// </remarks>
        /// <returns></returns>
        [HttpGet(nameof(Index))]
        [AllowAnonymous]
        public string Index()
        {
            return "Index api";
        }
        /// <summary>
        /// Get an Current Document NameSpace.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/v1/values/secureindex
        ///     
        /// </remarks>

        [HttpGet(nameof(SecureIndex))]
        public string SecureIndex()
        {
            this.logger.LogDebug("Start secure index of Controller");
            string indexValue = "Secure index";
            this.logger.LogInformation("Secure index of Controller", indexValue);
            return indexValue;
        }
    }
}
