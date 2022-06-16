using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using CosmosServices.Abstraction.Contracts;
using CosmosServices.Abstraction.Models;
using Cosmos.Common.Exceptions;
using CosmosRepositories.Abstraction.Contracts;

namespace CosmosCleanArchitecture.FunctionApp.Functions
{
    public class SaveDataToCosmos
    {
        private readonly ICosmosService _cosmosService;
        private readonly ILogRepository<Exception> _exceptionLogger;

        public SaveDataToCosmos(ICosmosService cosmosService, ILogRepository<Exception> exceptionLogger)
        {
            this._cosmosService = cosmosService;
            this._exceptionLogger = exceptionLogger;
        }
        [FunctionName("SaveDataToCosmos")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("SaveJsonToCosmos HTTP trigger function processed a request.");
            string Id = Guid.NewGuid().ToString();
            CancellationToken token = req.HttpContext.RequestAborted;
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                List<Users> data = JsonConvert.DeserializeObject<List<Users>>(requestBody);
                if (data.Count > 0)
                {
                    await _cosmosService.AddJsonListAsync(data, Id);
                }

                return new OkObjectResult($"{data.Count} Records inserted into cosmos with Id : {Id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while fetching user data");
                string exId = await this._exceptionLogger.LogAsync(ex, Id);

                return new BadRequestObjectResult($"Please contact customer support with the following identifier : {exId}.");
            }
        }
    }
}
