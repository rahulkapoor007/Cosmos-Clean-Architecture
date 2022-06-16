using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Abstraction.Models
{
    public class CosmosReadOperationLog
    {
        public CosmosReadOperationLog()
        {
            PartitionKey = DateTime.Now.ToString("yyyyMMdd");
            RowKey = Guid.NewGuid().ToString();
        }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string ContainerName { get; set; }
        public string OperationRequested { get; set; }
        public bool FromCache { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int RecordsReturned { get; set; }
        public double RequestCharges { get; set; }
    }
}
