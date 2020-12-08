using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xchangez.Helpers
{
    public class ErrorFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ErrorFilter> Logger;

        public ErrorFilter(ILogger<ErrorFilter> logger)
        {
            Logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            Logger.LogError(context.Exception, context.Exception.Message);
            base.OnException(context);
        }
    }
}
