using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyTodo.Api.Core
{
    public interface ITableStorageDataService<T>
    {
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task<T> GetAsync(Guid id);
        Task<IEnumerable<T>> QueryAsync(string query);
        Task DeleteAsync(T item);
    }
}
