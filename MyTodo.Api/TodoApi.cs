using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MyTodo.Shared;
using MyTodo.Api.Core;
using MyTodo.Api.Data;
using System;
using System.Text.Json;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace MyTodo.Api
{
    public static class Extensions
    {
        public static string GetUsername(this JwtTokenValidationResult result)
        {
            return result.Identity.Claims.Single(x => x.Type == "emails").Value;
        }
    }

    public class TodoApi
    {
        private readonly IJwtTokenValidator _jwtTokenValidator;
        private readonly ITableStorageDataService<TodoItem> _repository;

        public TodoApi(IJwtTokenValidator jwtTokenValidator, ITableStorageDataService<TodoItem> repository)
        {
            _jwtTokenValidator = jwtTokenValidator;
            _repository = repository;
        }

        [Function("AddTodo")]
        public async Task<HttpResponseData> AddTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequestData req)
        {

            var result = await _jwtTokenValidator.Validate(req);
            if (!result.Success)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var model = JsonSerializer.Deserialize<TodoModel>(body);

            await _repository.AddAsync(new TodoItem
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Description = model.Description,
                Username = result.GetUsername()
            });

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        [Function("UpdateTodo")]
        public async Task<HttpResponseData> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequestData req,
            string id)
        {

            var result = await _jwtTokenValidator.Validate(req);
            if (!result.Success)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }

            var model = JsonSerializer.Deserialize<TodoModel>(await new StreamReader(req.Body).ReadToEndAsync());

            var item = await _repository.GetAsync(Guid.Parse(id));
            item.IsDone = model.IsDone == true;
            item.LastUpdated = DateTime.UtcNow;

            await _repository.UpdateAsync(item);

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        [Function("DeleteTodo")]
        public async Task<HttpResponseData> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequestData req,
            string id)
        {
            var result = await _jwtTokenValidator.Validate(req);
            if (!result.Success)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }

            await _repository.DeleteAsync(new TodoItem
            {
                Id = Guid.Parse(id),
                Username = result.GetUsername()
            });

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        [Function("ListTodo")]
        public async Task<HttpResponseData> ListTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequestData req)
        {
            var result = await _jwtTokenValidator.Validate(req);
            if (!result.Success)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }

            List<TodoItem> items = (await _repository.QueryAsync($"Username eq '{result.GetUsername()}'")).ToList();
            List<TodoModel> models = items.Select(x => new TodoModel
            {
                Id = x.Id,
                Created = x.Created,
                Description = x.Description,
                IsDone = x.IsDone
            }).ToList();

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            var json = JsonSerializer.Serialize(models);
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return response;
        }
    }
}
