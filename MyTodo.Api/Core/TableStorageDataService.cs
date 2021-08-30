using FastMember;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTodo.Api.Core
{
    public class TableStorageDataService<T> : ITableStorageDataService<T>
    {
        private readonly CloudTable _cloudTable;

        public TableStorageDataService(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration["TableStorageConnection"]);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            var resourceType = typeof(T).Name;

            _cloudTable = cloudTableClient.GetTableReference(resourceType);
        }
        public async Task AddAsync(T item)
        {
            await _cloudTable.ExecuteAsync(TableOperation.Insert(Convert(item)));
        }

        public async Task UpdateAsync(T item)
        {
            await _cloudTable.ExecuteAsync(TableOperation.Merge(Convert(item)));
        }

        private DynamicTableEntity Convert(T item)
        {
            var ta = TypeAccessor.Create(typeof(T));
            var hasId = ta.GetMembers().SingleOrDefault(x => x.Name == "Id");

            if (hasId == null) throw new NotImplementedException("Model does not contain an Id property.");

            string key = ta[item, "Id"].ToString();

            var partitionKey = ta[item, "Username"].ToString();

            var dynamicTableEntity = new DynamicTableEntity(partitionKey, key)
            {
                Properties = EntityPropertyConverter.Flatten(item, new OperationContext())
            };
            dynamicTableEntity.ETag = "*";
            return dynamicTableEntity;
        }

        public async Task<T> GetAsync(Guid id)
        {
            var results = await Query($"Id eq guid'{id}'");
            return results.SingleOrDefault();
        }

        public async Task<IEnumerable<T>> QueryAsync(string query)
        {
            await _cloudTable.CreateIfNotExistsAsync();
            return await Query(query);
        }

        private async Task<IEnumerable<T>> Query(string queryString)
        {
            var list = new List<T>();

            var continuationToken = default(TableContinuationToken);

            do
            {
                var results = await _cloudTable.ExecuteQuerySegmentedAsync(new TableQuery { FilterString = queryString }, continuationToken);

                list.AddRange(results.Results.Select(x => EntityPropertyConverter.ConvertBack<T>(x.Properties, new OperationContext())).ToList());

                continuationToken = results.ContinuationToken;
            }
            while (continuationToken != null);


            return list;
        }

        public async Task DeleteAsync(T item)
        {
            await _cloudTable.ExecuteAsync(TableOperation.Delete(Convert(item)));
        }
    }
}
