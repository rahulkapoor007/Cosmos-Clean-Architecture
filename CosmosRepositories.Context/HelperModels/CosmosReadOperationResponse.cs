using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.HelperModels
{
    public class CosmosReadOperationResponse<T>
    {
        public int TotalDocuments { get; set; } = 0;
        public double TotalRequestUnitsConsumed { get; set; } = 0;
        public List<T> ListResult { get; set; }
        public T Result { get; set; }
        public string PartitionKey { get; set; }
        public bool FromCache { get; set; } = false;
    }
}
