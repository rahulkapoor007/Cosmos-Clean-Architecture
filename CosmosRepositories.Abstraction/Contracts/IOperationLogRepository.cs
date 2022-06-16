using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Abstraction.Contracts
{
    public interface IOperationLogRepository<T>
    {
        Task LogAsync(T log);
    }
}
