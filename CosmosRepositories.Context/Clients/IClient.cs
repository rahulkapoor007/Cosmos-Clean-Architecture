using CosmosRepositories.Context.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Clients
{
    public interface IClient<T>
    {
        Task<CosmosReadOperationResponse<int>> Count(string query, string partitionKey, CancellationToken token);
        Task<CosmosReadOperationResponse<T>> GetResult(string query, string partitionKey, CancellationToken token, int maxItemCount = 100);
    }
}
