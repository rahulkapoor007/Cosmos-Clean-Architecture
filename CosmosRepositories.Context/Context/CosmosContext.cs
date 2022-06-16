using CosmosRepositories.Context.CosmosEntites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Context.Context
{
    public class CosmosContext
    {
        public CosmosContext(CosmosDbContainerFactory containerFactory)
        {
            PropertyInfo[] propertyInfo = this.GetType().GetProperties();
            foreach (var info in propertyInfo)
            {
                info.SetValue(this, Activator.CreateInstance(info.PropertyType, containerFactory.GetContainer(info.Name)));
            }
        }
        //Note: These variables should always be the name of the container in the cosmos DB
        public CosmosDbSet<UserByCreatedDate> UserByCreatedDate { get; set; }

    }
}
