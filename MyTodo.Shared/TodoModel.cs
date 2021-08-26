using System;
using System.Text.Json.Serialization;

namespace MyTodo.Shared
{
    public class TodoModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("isDone")]
        public bool? IsDone { get; set; }
    }
}
