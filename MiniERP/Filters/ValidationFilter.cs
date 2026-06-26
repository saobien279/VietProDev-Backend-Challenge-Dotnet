using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniERP.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var argumentType = argument.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    // Create validation context
                    var validationContext = new ValidationContext<object>(argument);

                    // If route contains 'id', pass it to RootContextData for unique check bypass during updates
                    if (context.RouteData.Values.TryGetValue("id", out var idVal) && idVal != null)
                    {
                        if (int.TryParse(idVal.ToString(), out int intId))
                        {
                            validationContext.RootContextData["Id"] = intId;
                        }
                        else if (Guid.TryParse(idVal.ToString(), out Guid guidId))
                        {
                            validationContext.RootContextData["Id"] = guidId;
                        }
                        else
                        {
                            validationContext.RootContextData["Id"] = idVal;
                        }
                    }

                    var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
                    if (!validationResult.IsValid)
                    {
                        context.Result = new BadRequestObjectResult(new
                        {
                            success = false,
                            message = "Validation failed.",
                            data = (object?)null,
                            errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                        });
                        return; // Short-circuit the request
                    }
                }
            }

            await next();
        }
    }
}
