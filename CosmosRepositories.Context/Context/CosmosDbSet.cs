using Cosmos.Common.Exceptions;
using Cosmos.Common.Extensions;
using CosmosRepositories.Context.HelperModels;
using Microsoft.Azure.Cosmos;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Context
{
    public class CosmosDbSet<T>
    {
        private readonly Container _container;
        private volatile WrapperVolatileDouble volatileData;
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
        public CosmosDbSet(Container container)
        {
            this._container = container;
        }
        public async Task<CosmosWriteOperationResponse> BulkInsert(List<T> documents, string partitionKey)
        {
            if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new ApplicationErrorException(new Exception("Service is currently unavailable."));
            }
            string[] partitionProps = partitionKey.Split('.');
            object helperObj = new object();
            volatileData = new WrapperVolatileDouble();
            List<Exception> exceptions = new List<Exception>();

            Stopwatch stopwatch = Stopwatch.StartNew();
            CosmosWriteOperationResponse overallResponse = new CosmosWriteOperationResponse();
            int capacity = int.TryParse(Environment.GetEnvironmentVariable("BatchSize"), out int val) ? val : 1000;
            IEnumerable<IEnumerable<T>> ListOfBatches = ListExtensions.BatchBy(documents, capacity);
            foreach (List<T> batches in ListOfBatches)
            {
                int FailedCount = 0;
                int DuplicateCount = 0;
                int SuccessCount = 0;
                volatileData.RUCharge = 0;
                int AmountToInsert = batches.Count();
                List<Task> tasks = new List<Task>(AmountToInsert);
                foreach (T batch in batches)
                {
                    helperObj = batch;
                    foreach (string prop in partitionProps.ToList())
                    {
                        PropertyInfo property = helperObj.GetType().GetProperty(prop);
                        helperObj = property.GetValue(helperObj, null);
                    }

                    tasks.Add(_resilientPolicy.ExecuteAsync(async () => {
                        return await this._container.CreateItemAsync(batch, new PartitionKey(helperObj.ToString()));
                    }).ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {
                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                volatileData.RUCharge += cosmosException.RequestCharge;
                                if (cosmosException.StatusCode != System.Net.HttpStatusCode.Conflict)
                                {
                                    Interlocked.Increment(ref FailedCount);
                                    exceptions.Add(innerExceptions);
                                }
                                else
                                    Interlocked.Increment(ref DuplicateCount);

                            }
                            else
                            {
                                Interlocked.Increment(ref FailedCount);
                                exceptions.Add(innerExceptions);
                            }
                        }
                        else
                        {
                            volatileData.RUCharge += itemResponse.Result.RequestCharge;
                            Interlocked.Increment(ref SuccessCount);
                        }
                    }));
                }
                // Wait until all are done
                await Task.WhenAll(tasks);

                overallResponse.SuccessfulDocuments += SuccessCount;
                overallResponse.FailedDocuments += FailedCount;
                overallResponse.DuplicateDocuments += DuplicateCount;
                overallResponse.TotalRequestUnitsConsumed += volatileData.RUCharge;
            }

            stopwatch.Stop();
            overallResponse.TotalTimeTaken = stopwatch.Elapsed;
            overallResponse.TotalDocuments = documents.Count();
            overallResponse.SummarizedException = (exceptions != null && exceptions.Count > 0) ? await this.GetSummerizedExceptionAsync(exceptions) : null;
            return overallResponse;
        }
        private async Task<Exception> GetSummerizedExceptionAsync(List<Exception> exs)
        {
            try
            {
                StringBuilder exceptionMessage = new StringBuilder();
                foreach (Exception e in exs)
                {
                    exceptionMessage.Append(e.Message + "__");
                }
                if (exceptionMessage == null) return null;
                else throw new Exception(exceptionMessage.ToString());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
    public class WrapperVolatileDouble
    {
        public double RUCharge { get; set; }
    }
}
