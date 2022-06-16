using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Context
{
    public class CosmosDbContainerFactory
    {
        /// <summary>
        ///     Azure Cosmos DB Client
        /// </summary>
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;
        private readonly List<string> _containers;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="cosmosClient"></param>
        /// <param name="databaseName"></param>
        /// <param name="containers"></param>
        public CosmosDbContainerFactory(CosmosClient cosmosClient,
                                   string databaseName,
                                   List<string> containers)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _containers = containers ?? throw new ArgumentNullException(nameof(containers));
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        }

        public Container GetContainer(string containerName)
        {
            if (_containers.Where(x => x == containerName) == null)
            {
                throw new ArgumentException($"Unable to find container: {containerName}");
            }
            return _cosmosClient.GetContainer(_databaseName, containerName);
        }
    }
}
