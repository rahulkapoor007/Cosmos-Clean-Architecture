using Cosmos.Common.Models;
using CosmosRepositories.Abstraction.Models;
using CosmosRepositories.Context.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Abstraction.Contracts
{
    public interface IRepository<T> where T : class
    {
        Task<CosmosWriteOperationResponse> AddAsync(List<T> model);
        Task<CosmosReadOperationResponse<T>> GetIemsAsync
            (DateTime PartitionKey, List<SearchModel> searchItems,
            KeyValuePair<string, string> sortingFieldAndOrder,
            KeyValuePair<int, int> pageSizeAndNumber, CancellationToken token);
    }
}
