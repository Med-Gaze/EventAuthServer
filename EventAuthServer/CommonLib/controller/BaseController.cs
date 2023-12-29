using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace med.common.library.controller
{
    /// <summary>
    ///  custom controller without url versioning
    /// </summary>
    [ApiController]
    [Route("api/v{api-version:apiVersion}/[Controller]")]
    public abstract class BaseController : ControllerBase
    {
        private ISender _mediator = null!;
        /// <summary>
        /// event driven design for api 
        /// </summary>
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    }

}
