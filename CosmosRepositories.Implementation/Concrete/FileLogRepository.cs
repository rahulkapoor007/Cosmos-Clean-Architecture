using Cosmos.Common.Exceptions;
using CosmosRepositories.Abstraction.Contracts;
using CosmosRepositories.Abstraction.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Implementation.Concrete
{
    public class FileLogRepository : ILogRepository<Exception>, IOperationLogRepository<CosmosWriteOperationLog>
        , IOperationLogRepository<CosmosReadOperationLog>
    {
        public async Task<string> LogAsync(Exception ex, string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id)) Id = Guid.NewGuid().ToString();
                ApplicationErrorException applicationException;
                if (ex is ApplicationErrorException) { applicationException = ex as ApplicationErrorException; }
                else { applicationException = new ApplicationErrorException(ex); }

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Identifier: {Id}");
                builder.AppendLine($"Raised Time: {applicationException.RaisedTime.ToString()}");
                builder.AppendLine($"Message: {applicationException.Message}");
                builder.AppendLine($"Exception: {applicationException.ApplicationException?.ToString() ?? string.Empty}");
                builder.AppendLine($"Inner Exception: {((applicationException.InnerException != null) ? applicationException.InnerException.Message : string.Empty)}");
                builder.AppendLine($"Stack Trace: {applicationException.StackTrace?.ToString() ?? string.Empty}");

                bool result = await LogExceptionIntoFile(builder.ToString(), "Exception");
                if (result) return Id;
                else return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task LogAsync(CosmosWriteOperationLog log)
        {
            try
            {
                string message = JsonConvert.SerializeObject(log);
                await LogExceptionIntoFile(message, "WriteOperationLogs");
            }
            catch (Exception ex)
            { }
        }
        public async Task LogAsync(CosmosReadOperationLog log)
        {
            try
            {
                string message = JsonConvert.SerializeObject(log);
                await LogExceptionIntoFile(message, "ReadOperationLogs");
            }
            catch (Exception ex)
            { }
        }

        #region Private Methods
        private async Task<bool> LogExceptionIntoFile(string message, string fileName)
        {
            string currentDay = DateTime.UtcNow.ToString("dd-MM-yyyy");

            using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(currentDay + $"_{fileName}.txt"))
            {
                await streamWriter.WriteAsync(String.Format("\r\n\r\n\r\n{0}", message));
                return true;
            }
        }
        #endregion
    }
}
