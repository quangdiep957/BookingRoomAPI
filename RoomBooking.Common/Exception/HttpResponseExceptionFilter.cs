using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Exception
{
    public class HttpResponseExceptionFilter : IOrderedFilter, Microsoft.AspNetCore.Mvc.Filters.IActionFilter
    {
        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                if (context.Exception is ValidateException)
                {
                    var res = new
                    {

                        userMsg =Resources.Resource.Error_DataInputIsError,
                        data = context.Exception.Data
                    };
                    context.Result = new ObjectResult(res)
                    {
                        StatusCode = 400,

                    };
                    context.ExceptionHandled = true;

                }
                else
                {
                    var res = new
                    {

                        userMsg = Resources.Resource.Error,
                        data = context.Exception.Data,

                    };
                    context.Result = new ObjectResult(res)
                    {
                        StatusCode = 500,

                    };
                    context.ExceptionHandled = true;
                }
            }
        }

    }
}
