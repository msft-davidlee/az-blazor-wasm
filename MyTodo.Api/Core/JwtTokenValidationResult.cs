using System.Security.Claims;

namespace MyTodo.Api.Core
{
    public class JwtTokenValidationResult
    {
        public JwtTokenValidationResult(bool success, ClaimsPrincipal identity = null)
        {
            Success = success;
            Identity = identity;
        }
        public bool Success { get; }
        public ClaimsPrincipal Identity { get; }
    }
}
