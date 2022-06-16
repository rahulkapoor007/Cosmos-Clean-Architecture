using Cosmos.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Models
{
    public class SearchFields
    {
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }

        [JsonProperty("fieldValue")]
        public List<string> FieldValue { get; set; }

        [JsonProperty("operatorType")]
        public Operator OperatorType { get; set; }
    }
}
