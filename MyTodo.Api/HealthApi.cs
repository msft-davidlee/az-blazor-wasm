using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MyTodo.Api
{
    public class HealthApi
    {
        [Function("Ping")]
        public HttpResponseData Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ping")] HttpRequestData req,
            FunctionContext context)
        {
            context.LogInformation("Ping");

            var model = new
            {
                Value = "pong",
                Timestamp = DateTime.UtcNow,
                Server = Environment.MachineName,
                OS = Environment.OSVersion
            };

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            var json = JsonSerializer.Serialize(model);
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return response;
        }
    }
}
