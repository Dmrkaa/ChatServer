using ChatServer.Application.Controllers;
using ChatServer.SharedModels;
using System.Threading.Tasks;

namespace ChatServer.Application.Services
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginModel login);
        string CreateToken();
    }
}
