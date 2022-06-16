using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosServices.Abstraction.Models
{
    public class UserResponseModel
    {
        [JsonProperty("userList")]
        public List<Users> UserList { get; set; } = new List<Users>();

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonProperty("resultCount")]
        public int ResultCount { get; set; } = 0;
    }
}
