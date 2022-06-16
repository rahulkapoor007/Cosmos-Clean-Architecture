using Cosmos.Common.Exceptions;
using Cosmos.Common.Extensions;
using Cosmos.Common.Models;
using CosmosRepositories.Abstraction.Contracts;
using CosmosRepositories.Abstraction.Models;
using CosmosRepositories.Context.CosmosEntites;
using CosmosRepositories.Context.HelperModels;
using CosmosServices.Abstraction.Contracts;
using CosmosServices.Abstraction.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosServices.Implementation.Concrete
{
    public class CosmosService : ICosmosService
    {
        private readonly IRepository<UserByCreatedDate> _repository;
        IOperationLogRepository<CosmosWriteOperationLog> _writeLogRepository;
        IOperationLogRepository<CosmosReadOperationLog> _readLogRepository;
        private readonly ILogger<CosmosService> _logger;
        public CosmosService(IRepository<UserByCreatedDate> repository, 
            IOperationLogRepository<CosmosWriteOperationLog> operationLogRepository, ILogger<CosmosService> logger,
            IOperationLogRepository<CosmosReadOperationLog> readLogRepository)
        {
            this._repository = repository;
            this._writeLogRepository = operationLogRepository;
            this._logger = logger;
            this._readLogRepository = readLogRepository;
        }
        public async Task AddJsonListAsync(List<Users> userList, string Id)
        {
            try
            {
                List<UserByCreatedDate> entityList = this.AssignToEntity(userList, Id);
                if (entityList.Count > 0)
                {
                    var result = await this._repository.AddAsync(entityList);
                    await this.AddOperationLogAsync(result, Id, "UserByCreatedDate");
                    if (result.FailedDocuments > 0)
                    {
                        if (result.SummarizedException != null)
                            throw new ApplicationErrorException(result.SummarizedException);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
        public async Task<UserResponseModel> GetUser(UserRequestModel model, CancellationToken token)
        {
            try
            {
                //******Code to convert selector model into repository parameters******//
                List<SearchModel> searchModelList = model.SearchFields == null ? null : SearchFieldExtensions.GetSearchFields<Users>(model.SearchFields);
                //string pipeDuns = model.TspCode;
                model.PageNumber = model.PageNumber - 1;
                KeyValuePair<string, string> sortingFielsAndDirection = new KeyValuePair<string, string>(model.SortingField.FieldName, model.SortingField.Direction);
                KeyValuePair<int, int> pageNumberAndCount = new KeyValuePair<int, int>(model.PageSize, model.PageNumber * model.PageSize);
                //*******************************************************************//

                CosmosReadOperationResponse<UserByCreatedDate> operationResponse = await this._repository.GetIemsAsync(model.UserByCreatedDate, searchModelList, sortingFielsAndDirection, pageNumberAndCount, token);

                //*********Log the operation response in a storage table*************//
                CosmosReadOperationLog operationLog = new CosmosReadOperationLog()
                {
                    ContainerName = "UserByCreatedDate",
                    OperationRequested = "Search",
                    PageNumber = model.PageNumber++,
                    PageSize = model.PageSize,
                    RecordsReturned = operationResponse.ListResult.Count(),
                    RequestCharges = operationResponse.TotalRequestUnitsConsumed,
                    TotalRecords = operationResponse.TotalDocuments,
                    FromCache = operationResponse.FromCache
                };
                _ = Task.Run(() =>
                {
                    try { this._readLogRepository.LogAsync(operationLog); }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error in LogAsync() " +
                            $"\nMessage : {e.Message} " +
                            $"\nStackTrace: {e.StackTrace}" +
                            $"\nInner Exception: {e.InnerException}");
                    }
                });
                //********************************************************************//

                if (operationResponse.ListResult.Count == 0) return null;

                UserResponseModel userModel = new UserResponseModel();
                userModel.UserList = this.AssignToEntity(operationResponse.ListResult);
                userModel.PageIndex = model.PageNumber;
                userModel.PageSize = model.PageSize;
                userModel.TotalCount = operationResponse.TotalDocuments;
                userModel.ResultCount = userModel.UserList == null ? 0 : userModel.UserList.Count();

                return userModel;
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
        }
        #region Private Methods
        private List<UserByCreatedDate> AssignToEntity(List<Users> modelList, string Id)
        {
            HashSet<UserByCreatedDate> entityList = new HashSet<UserByCreatedDate>(new UserByCreatedDateComparer());
            try
            {
                if (modelList.Count > 0)
                {
                    var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
                    Parallel.ForEach((modelList), model => {
                        try
                        {
                            UserByCreatedDate entity = new UserByCreatedDate();
                            entity.Id = Guid.NewGuid().ToString();
                            entity.TransactionId = Id;
                            entity.FirstName = model.FirstName;
                            entity.LastName = model.LastName;
                            entity.Email = model.Email;
                            entity.Gender = model.Gender;
                            entity.IPAddress = model.IPAddress;
                            entity.UniqueKey = HashingExtensions.GetIdentifier(this.GetRecords(model));

                            lock(entityList)
                                entityList.Add(entity);
                        }
                        catch (Exception e)
                        {
                            exceptions.Enqueue(e);
                        }
                    });
                    if (exceptions != null && exceptions.Count > 0)
                    {
                        AggregateException aggregateException = new AggregateException(exceptions);
                        throw new ApplicationErrorException(aggregateException);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ApplicationErrorException) throw;
                else throw new ApplicationErrorException(ex);
            }
            return entityList?.ToList();
        }
        private UserIdentifiers GetRecords(Users user)
        {
            return new UserIdentifiers()
            {
                Email = user.Email
            };
        }
        private async Task AddOperationLogAsync(CosmosWriteOperationResponse bulkOperation, string Id, string containerName)
        {
            CosmosWriteOperationLog log = new CosmosWriteOperationLog(Id)
            {
                SuccessfulDocuments = bulkOperation.SuccessfulDocuments,
                ContainerName = containerName,
                TimeTaken = bulkOperation.TotalTimeTaken.TotalSeconds,
                RequestUnitsConsumed = bulkOperation.TotalRequestUnitsConsumed,
                FailedDocuments = bulkOperation.FailedDocuments,
                DuplicateDocuments = bulkOperation.DuplicateDocuments,
                TotalRecords = bulkOperation.TotalDocuments
            };
            await this._writeLogRepository.LogAsync(log);
        }
        private List<Users> AssignToEntity(List<UserByCreatedDate> modelList)
        {
            List<Users> result = new List<Users>();
            try
            {
                var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
                Parallel.ForEach(modelList, (model) => {
                    Users entity = new Users();
                    try
                    {
                        entity.FirstName = model.FirstName;
                        entity.LastName = model.LastName;
                        entity.Email = model.Email;
                        entity.Gender = model.Gender;
                        entity.IPAddress = model.IPAddress;

                        lock (result)
                            result.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
                if (exceptions != null && exceptions.Count > 0)
                {
                    AggregateException aggregateException = new AggregateException(exceptions);
                    throw new ApplicationErrorException(aggregateException);
                }
                return result;

            }
            catch (Exception ex)
            {
                throw new ApplicationErrorException(ex);
            }
        }
        #endregion
    }
}
