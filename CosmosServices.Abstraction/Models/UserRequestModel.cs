using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CosmosServices.Abstraction.Models
{
    public class UserRequestModel
    {
        [JsonProperty("userByCreatedDate")]
        [Required]
        public DateTime UserByCreatedDate { get; set; }

        [JsonProperty("searchFields")]
        public List<SearchFields> SearchFields { get; set; } = new List<SearchFields>();

        [JsonProperty("sortingField")]
        public SortingField SortingField { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }
}
