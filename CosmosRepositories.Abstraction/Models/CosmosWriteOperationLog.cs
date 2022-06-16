using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Abstraction.Models
{
    public class CosmosWriteOperationLog
    {
        public CosmosWriteOperationLog(string rowKey)
        {
            PartitionKey = DateTime.Now.ToString("yyyyMMdd");
            RowKey = rowKey;
        }
        public string RowKey { get; set; }
        public string PartitionKey { get; private set; }
        public string Id { get; set; }
        public string ContainerName { get; set; }
        public double TimeTaken { get; set; }
        public int SuccessfulDocuments { get; set; } = 0;
        public int FailedDocuments { get; set; } = 0;
        public int DuplicateDocuments { get; set; } = 0;
        public double RequestUnitsConsumed { get; set; } = 0;
        public int TotalRecords { get; set; }


    }
}
