using MyTodo.Api.Core;
using System.Linq;

namespace MyTodo.Api
{
    public static class Extensions
    {
        public static string GetUsername(this JwtTokenValidationResult result)
        {
            return result.Identity.Claims.Single(x => x.Type == "emails").Value;
        }
    }
}
