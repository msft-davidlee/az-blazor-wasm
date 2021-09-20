using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyTodo.Api.Core;
using System;
using System.Linq;

namespace MyTodo.Api
{
    public static class Extensions
    {
        public static string GetUsername(this JwtTokenValidationResult result)
        {
            return result.Identity.Claims.Single(x => x.Type == "emails").Value;
        }

        public static void LogInformation(this FunctionContext context, string message)
        {
            var logger = context.GetLogger("TodoApi");
            logger.LogInformation($"[{DateTime.UtcNow}] {message}");
        }
    }
}
