using Cosmos.Common.Exceptions;
using Cosmos.Common.Extensions;
using CosmosRepositories.Context.CosmosEntites;
using CosmosRepositories.Context.HelperModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Clients.CachedClient
{
    public class CachedUserClient : IClient<UserByCreatedDate>
    {
        private readonly IClient<UserByCreatedDate> _client;
        private readonly IMemoryCache _memoryCache;

        public CachedUserClient(IClient<UserByCreatedDate> client, IMemoryCache memoryCache)
        {
            _client = client;
            _memoryCache = memoryCache;
        }
        public async Task<CosmosReadOperationResponse<int>> Count(string query, string partitionKey, CancellationToken token)
        {
            try
            {
                string recordId = HashingExtensions.GetIdentifier(query);
                var data = MemoryCacheExtensions.GetRecordAsync<int>(_memoryCache, recordId, true);
                if (data == 0)
                {
                    var dataFromRepo = await _client.Count(query, partitionKey, token);
                    MemoryCacheExtensions.SetRecordAsync(_memoryCache, recordId, dataFromRepo.TotalDocuments, true, TimeSpan.FromSeconds(60));
                    return dataFromRepo;
                }

                return new CosmosReadOperationResponse<int>() { TotalDocuments = data, PartitionKey = partitionKey, FromCache = true };
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }

        }
        public async Task<CosmosReadOperationResponse<UserByCreatedDate>> GetResult(string query, string partitionKey, CancellationToken token, int maxItemCount = 100)
        {
            try
            {
                string recordId = HashingExtensions.GetIdentifier(query);
                List<UserByCreatedDate> data = MemoryCacheExtensions.GetRecordAsync<List<UserByCreatedDate>>(_memoryCache, recordId, true);
                if (data is null)
                {
                    var dataFromRepo = await _client.GetResult(query, partitionKey, token, maxItemCount);
                    MemoryCacheExtensions.SetRecordAsync(_memoryCache, recordId, dataFromRepo.ListResult, true, TimeSpan.FromSeconds(60));
                    return dataFromRepo;
                }

                return new CosmosReadOperationResponse<UserByCreatedDate>() { ListResult = data, PartitionKey = partitionKey, FromCache = true };
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
    }
}
