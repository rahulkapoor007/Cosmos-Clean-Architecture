using CosmosRepositories.Context.Context;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosCleanArchitecture.FunctionApp.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Register a singleton instance of Cosmos Db Container Factory, which is a wrapper for the CosmosClient.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="primaryKey"></param>
        /// <param name="databaseName"></param>
        /// <param name="containers"></param>
        /// <returns></returns>
        public static async Task<IServiceCollection> AddCosmosDbAsync(this IServiceCollection services,
                                                     string CosmosEndpoint,
                                                     string CosmosMasterKey,
                                                     string databaseName,
                                                     List<string> containers)
        {
            List<(string databaseId, string containerId)> _containers = new List<(string databaseId, string containerId)>();
            foreach (var container in containers) _containers.Add((databaseName, container));
            CosmosClientOptions clientOptions = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60),
                AllowBulkExecution = true
            };
            //Always try to create cosmos client with this method because this method help in cold start the applicationfirst time
            //This method caches and connections before the first call is made to the service
            CosmosClient client = await CosmosClient.CreateAndInitializeAsync(CosmosEndpoint, CosmosMasterKey, _containers, clientOptions);

            CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName, containers);
            // Microsoft recommends a singleton client instance to be used throughout the application
            // "CosmosClient is thread-safe. Its recommended to maintain a single instance of CosmosClient per lifetime of the application
            // which enables efficient connection management and performance"
            services.AddSingleton<CosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }
    }
}
