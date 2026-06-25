using Microsoft.AspNetCore.Mvc.Filters;
using MiniERP.Application.Interfaces;

namespace MiniERP.Filters
{
    public class NormalizeFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is INormalizable normalizable)
                {
                    normalizable.Normalize();
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing
        }
    }
}
