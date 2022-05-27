using ChatServer.Domain.Entities;
using System.Threading.Tasks;

namespace ChatServer.Domain.Repository_Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> CreateRoom(string roomName);
        Task DeleteRoom(int roomId);
        Task<Room> GetRoomByUserName(string userName);
        Task<Room> GetRoom(int roomId);
        Task<Room> GetRoom(string roomName);
    }
}
