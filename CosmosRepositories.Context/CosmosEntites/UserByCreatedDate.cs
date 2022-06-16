using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.CosmosEntites
{
    public class UserByCreatedDate
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = DateTime.Now.ToString("yyyyMMdd");

        [JsonProperty(PropertyName = "uniqueKey")]
        public string UniqueKey { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }
        [JsonProperty(PropertyName = "ipAddress")]
        public string IPAddress { get; set; }
        [JsonProperty(PropertyName = "createdDateTime")]
        public DateTime? CreatedDateTime { get; set; } = DateTime.Now;
    }
    public class UserByCreatedDateComparer : IEqualityComparer<UserByCreatedDate>
    {
        public bool Equals(UserByCreatedDate value1, UserByCreatedDate value2)
        {
            if (value2 == null && value1 == null)
                return true;
            else if (value1 == null || value2 == null)
                return false;
            else if (value1.UniqueKey.Trim().Equals(value2.UniqueKey.Trim()))
                return true;
            else
                return false;
        }

        public int GetHashCode(UserByCreatedDate value)
        {
            return value.UniqueKey.GetHashCode();
        }
    }
}
