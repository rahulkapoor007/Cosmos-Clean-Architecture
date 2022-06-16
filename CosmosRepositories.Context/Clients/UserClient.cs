using Cosmos.Common.Exceptions;
using CosmosRepositories.Context.Context;
using CosmosRepositories.Context.CosmosEntites;
using CosmosRepositories.Context.HelperModels;
using Microsoft.Azure.Cosmos;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Clients
{
    public class UserClient : IClient<UserByCreatedDate>
    {
        private readonly CosmosDbContainerFactory _containerFactory;
        private const int maxRetries = 3;
        private static readonly Random randomDelay = new Random();

        //************************Retry Policy******************************
        private static readonly AsyncRetryPolicy _transientErrorRetryPolicy =
            Policy.Handle<CosmosException>(
                message => ((int)message.StatusCode == 429 || (int)message.StatusCode == 408
                || (int)message.StatusCode > 500))
                .WaitAndRetryAsync(maxRetries, retryAttempt =>
                {
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    + TimeSpan.FromMilliseconds(randomDelay.Next(0, 1000));
                });
        //******************************************************************

        //************************Circuit Breaker****************************
        private static readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy =
            Policy.Handle<CosmosException>(message => (int)message.StatusCode == 429)
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 100, TimeSpan.FromMinutes(1));
        private readonly AsyncPolicyWrap _resilientPolicy =
            _circuitBreakerPolicy.WrapAsync(_transientErrorRetryPolicy);

        //********************************************************************

        public UserClient(CosmosDbContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        public async Task<CosmosReadOperationResponse<int>> Count(string query, string partitionKey, CancellationToken token)
        {
            if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new ApplicationErrorException(new Exception("Service is currently unavailable."));
            }
            try
            {
                Container _container = _containerFactory.GetContainer(Environment.GetEnvironmentVariable("Container:User"));
                double RUCharges = 0;
                int totalCount = default(int);
                QueryRequestOptions requestOptions = new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = 10
                };
                QueryDefinition queryDefinition = new QueryDefinition(query);
                FeedIterator<int> queryResultSetIterator = _container.GetItemQueryIterator<int>(queryDefinition, null, requestOptions);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<int> currentResultSet = await _resilientPolicy.ExecuteAsync
                            (async () => await queryResultSetIterator.ReadNextAsync(token));
                    RUCharges += currentResultSet.RequestCharge;
                    totalCount += currentResultSet.FirstOrDefault();
                }
                return new CosmosReadOperationResponse<int>
                {
                    TotalDocuments = totalCount,
                    TotalRequestUnitsConsumed = RUCharges,
                    PartitionKey = partitionKey
                };
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
        public async Task<CosmosReadOperationResponse<UserByCreatedDate>> GetResult(string query, string partitionKey, CancellationToken token, int maxItemCount = 100)
        {
            if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new ApplicationErrorException(new Exception("Service is currently unavailable."));
            }
            try
            {
                Container _container = _containerFactory.GetContainer(Environment.GetEnvironmentVariable("Container:User"));
                CosmosReadOperationResponse<UserByCreatedDate> response = new CosmosReadOperationResponse<UserByCreatedDate>();
                QueryDefinition queryDefinition = new QueryDefinition(query);

                QueryRequestOptions requestOptions = new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = maxItemCount
                };
                FeedIterator<UserByCreatedDate> queryResultSetIterator = _container.GetItemQueryIterator<UserByCreatedDate>(queryDefinition, null, requestOptions);
                List<UserByCreatedDate> results = new List<UserByCreatedDate>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<UserByCreatedDate> currentResultSet = await _resilientPolicy.ExecuteAsync
                        (async () => await queryResultSetIterator.ReadNextAsync(token));

                    results.AddRange(currentResultSet);
                    response.TotalRequestUnitsConsumed += currentResultSet.RequestCharge;
                }
                response.TotalDocuments = results.Count;
                response.ListResult = results;
                response.PartitionKey = partitionKey;
                return response;
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
    }
}
