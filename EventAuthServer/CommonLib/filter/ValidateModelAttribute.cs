using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace med.common.library.filter
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var param = context.ActionArguments.SingleOrDefault();

            if (param.Value == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (!context.ModelState.IsValid)
            {

                string errors = context.ModelState.SelectMany(selector: state => state.Value.Errors).Aggregate("", (current, error) => current + (error.ErrorMessage + ". "));
                context.Result = new BadRequestObjectResult(errors);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something with Result.
            if (context.Canceled == true)
            {
                // Action execution was short-circuited by another filter.
            }

            if (context.Exception != null)
            {
                // Exception thrown by action or action filter.
                // Set to null to handle the exception.
                //context.Exception = null;
            }
            base.OnActionExecuted(context);
        }

    }
}
