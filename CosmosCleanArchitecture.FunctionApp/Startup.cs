using CosmosCleanArchitecture.FunctionApp.Extensions;
using CosmosRepositories.Abstraction.Contracts;
using CosmosRepositories.Abstraction.Models;
using CosmosRepositories.Context.Clients;
using CosmosRepositories.Context.Clients.CachedClient;
using CosmosRepositories.Context.Context;
using CosmosRepositories.Context.CosmosEntites;
using CosmosRepositories.Implementation.Concrete;
using CosmosServices.Abstraction.Contracts;
using CosmosServices.Implementation.Concrete;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(CosmosCleanArchitecture.FunctionApp.Startup))]
namespace CosmosCleanArchitecture.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
            ConfigureRepositories(builder.Services);
            CongigureContext(builder.Services);
            ConfigureCosmosServices(builder.Services);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICosmosService, CosmosService>();
        }
        public void ConfigureRepositories(IServiceCollection services)
        {
            services.AddScoped<ILogRepository<Exception>, FileLogRepository>();
            services.AddScoped<IOperationLogRepository<CosmosWriteOperationLog>, FileLogRepository>();
            services.AddScoped<IOperationLogRepository<CosmosReadOperationLog>, FileLogRepository>();
            services.AddScoped<IRepository<UserByCreatedDate>, CosmosRepository>();
        }
        public void CongigureContext(IServiceCollection services)
        {
            services.AddScoped<CosmosDbSet<UserByCreatedDate>>();
            services.AddScoped<CosmosContext>();
            services.AddSingleton<CosmosUserContext>();

            services.AddSingleton<IClient<UserByCreatedDate>, UserClient>();
            if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development")
            {
                if (Environment.GetEnvironmentVariable("EnableCachedSource") == "True")
                {
                    services.AddDistributedMemoryCache();//RESOLVING MEMORY CACHE
                    services.Decorate<IClient<UserByCreatedDate>, CachedUserClient>();//RESOLVING CACHE FOR CONTEXT
                }
            }
            else
            {
                //can add any other source of cache like Redis
            }
        }
        public void ConfigureCosmosServices(IServiceCollection services)
        {
            //Getting container names from local settings file
            HashSet<string> containers = new HashSet<string>();
            PropertyInfo[] properties = typeof(CosmosContext).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                containers.Add(property.Name);
            }
            // register CosmosDB client and data repositories
            services.AddCosmosDbAsync(Environment.GetEnvironmentVariable("CosmosEndpoint"),
                                 Environment.GetEnvironmentVariable("CosmosMasterKey"),
                                 Environment.GetEnvironmentVariable("DatabaseId"),
                                 containers.ToList()).Wait();
        }
    }
}
