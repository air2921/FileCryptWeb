using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using webapi.Exceptions;

namespace webapi.Attributes
{
    public class EntityExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            int statusCode = 500;
            string message = "Unexpected error";

            Type[] ex = [typeof(EntityNotCreatedException), typeof(EntityNotDeletedException),
            typeof(EntityNotUpdatedException), typeof(OperationCanceledException)];

            if (ex.Any(e => e.IsInstanceOfType(context.Exception)))
                message = context.Exception.Message;

            context.Result = new JsonResult(new { message = message })
            {
                StatusCode = statusCode
            };
        }
    }
}
