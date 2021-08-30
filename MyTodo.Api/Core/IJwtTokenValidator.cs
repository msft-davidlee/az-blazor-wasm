using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

namespace MyTodo.Api.Core
{
    public interface IJwtTokenValidator
    {
        Task<JwtTokenValidationResult> Validate(HttpRequestData httpRequest);
    }
}
