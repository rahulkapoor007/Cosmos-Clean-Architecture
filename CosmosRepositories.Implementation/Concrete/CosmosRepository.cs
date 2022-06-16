using Cosmos.Common.Exceptions;
using Cosmos.Common.Extensions;
using Cosmos.Common.Models;
using CosmosRepositories.Abstraction.Contracts;
using CosmosRepositories.Abstraction.Models;
using CosmosRepositories.Context.Context;
using CosmosRepositories.Context.CosmosEntites;
using CosmosRepositories.Context.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Implementation.Concrete
{
    public class CosmosRepository : IRepository<UserByCreatedDate>
    {
        private readonly CosmosContext _context;
        private readonly CosmosUserContext _cosmosUserContext;

        public CosmosRepository(CosmosContext context, CosmosUserContext cosmosUserContext)
        {
            this._context = context;
            this._cosmosUserContext = cosmosUserContext;
        }
        public async Task<CosmosWriteOperationResponse> AddAsync(List<UserByCreatedDate> model)
        {
            try
            {
                return await this._context.UserByCreatedDate.BulkInsert(model, "PartitionKey");
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }

        public async Task<CosmosReadOperationResponse<UserByCreatedDate>> GetIemsAsync
            (DateTime PartitionKey, List<SearchModel> searchItems, KeyValuePair<string, string> sortingFieldAndOrder,
            KeyValuePair<int, int> pageSizeAndNumber, CancellationToken token)
        {
            try
            {
                string Key = PartitionKey.ToString("yyyyMMdd");
                string query = $" WHERE c.partitionKey= '{Key}'";
                List<string> conditions = new List<string>();
                if (searchItems != null && searchItems.Count > 0)
                {
                    query += " AND ";
                    foreach (SearchModel searchItem in searchItems)
                    {
                        conditions.Add($"c.{searchItem.FieldName} {searchItem.FieldOperator.GetEnumDescription()} {searchItem.FieldValueString}");
                    }
                    query += string.Join(" AND ", conditions);
                }
                string countQuery = "SELECT VALUE COUNT(1) FROM c" + query;
                CosmosReadOperationResponse<int> countResponse = await this._cosmosUserContext.User.Count(countQuery, Key, token);
                if (countResponse.TotalDocuments == 0) return new CosmosReadOperationResponse<UserByCreatedDate>()
                {
                    TotalDocuments = 0,
                    TotalRequestUnitsConsumed = countResponse.TotalRequestUnitsConsumed,
                    ListResult = new List<UserByCreatedDate>()
                };


                query += $" ORDER BY c.{sortingFieldAndOrder.Key} {sortingFieldAndOrder.Value}";
                query += $" OFFSET {pageSizeAndNumber.Value} LIMIT {pageSizeAndNumber.Key}";
                query = $"SELECT {Environment.GetEnvironmentVariable("UserQuery:ForList")} FROM c {query}";

                CosmosReadOperationResponse<UserByCreatedDate> entityResponse = await this._cosmosUserContext.User.GetResult(query, Key, token);
                entityResponse.TotalDocuments = countResponse.TotalDocuments;
                entityResponse.TotalRequestUnitsConsumed += countResponse.TotalRequestUnitsConsumed;
                return entityResponse;
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
    }
}
