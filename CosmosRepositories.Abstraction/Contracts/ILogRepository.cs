using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosRepositories.Abstraction.Contracts
{
    public interface ILogRepository<T>
    {
        Task<string> LogAsync(T ex, string? Id);
    }
}
