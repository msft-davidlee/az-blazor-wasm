using System;

namespace MyTodo.Api.Data
{
    public class TodoItem
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime Created { get; set; }
    }
}
