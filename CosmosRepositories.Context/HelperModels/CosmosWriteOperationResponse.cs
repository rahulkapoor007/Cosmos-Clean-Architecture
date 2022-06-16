using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.HelperModels
{
    public class CosmosWriteOperationResponse
    {
        public TimeSpan TotalTimeTaken { get; set; }
        public int SuccessfulDocuments { get; set; } = 0;
        public int TotalDocuments { get; set; } = 0;
        public int FailedDocuments { get; set; } = 0;
        public int DuplicateDocuments { get; set; } = 0;
        public double TotalRequestUnitsConsumed { get; set; } = 0;
        public Exception SummarizedException { get; set; } = null;
    }
}
