using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Models
{
    public class SortingField
    {
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }
    }
}
