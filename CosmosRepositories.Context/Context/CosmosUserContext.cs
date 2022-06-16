using CosmosRepositories.Context.Clients;
using CosmosRepositories.Context.CosmosEntites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Context
{
    public class CosmosUserContext
    {
        public CosmosUserContext(IClient<UserByCreatedDate> User)
        {
            this.User = User;
        }

        public IClient<UserByCreatedDate> User { get; set; }
    }
}
