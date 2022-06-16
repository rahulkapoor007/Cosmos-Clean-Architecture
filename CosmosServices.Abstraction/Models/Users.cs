using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosServices.Abstraction.Models
{
    public class Users
    {
        [Description("c.firstName")]
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [Description("c.lastName")]
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [Description("c.email")]
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [Description("c.gender")]
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [Description("c.ipAddress")]
        [JsonProperty(PropertyName = "ipAddress")]
        public string IPAddress { get; set; }
    }
}
