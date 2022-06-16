using CosmosServices.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosServices.Abstraction.Contracts
{
    public interface ICosmosService
    {
        Task AddJsonListAsync(List<Users> userList, string Id);
        Task<UserResponseModel> GetUser(UserRequestModel data, CancellationToken token);
    }
}
