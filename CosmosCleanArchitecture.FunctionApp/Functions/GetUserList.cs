using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using CosmosServices.Abstraction.Models;
using CosmosServices.Abstraction.Contracts;
using CosmosRepositories.Abstraction.Contracts;

namespace CosmosCleanArchitecture.FunctionApp.Functions
{
    public class GetUserList
    {
        private readonly ICosmosService _cosmosService;
        private readonly ILogRepository<Exception> _exceptionLogger;

        public GetUserList(ICosmosService cosmosService, ILogRepository<Exception> exceptionLogger)
        {
            this._cosmosService = cosmosService;
            this._exceptionLogger = exceptionLogger;
        }
        [FunctionName("GetUserList")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            CancellationToken token = req.HttpContext.RequestAborted;
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UserRequestModel data = JsonConvert.DeserializeObject<UserRequestModel>(requestBody);
                return new OkObjectResult(await _cosmosService.GetUser(data, token));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while fetching user data");

                string exId = await this._exceptionLogger.LogAsync(ex, null);
                return new BadRequestObjectResult($"Please contact customer support with the following identifier : {exId}.");
            }
        }
    }
}
