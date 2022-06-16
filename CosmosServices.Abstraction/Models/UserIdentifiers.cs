using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosServices.Abstraction.Models
{
    /// <summary>
    /// This class is responsible for avoiding any
    /// duplicate records gettting inserted into 
    /// cosmos on the basis of email.
    /// </summary>
    public class UserIdentifiers
    {
        public string Email { get; set; }
    }
}
