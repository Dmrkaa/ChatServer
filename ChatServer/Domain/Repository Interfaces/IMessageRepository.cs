using ChatServer.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatServer.Domain.Repository_Interfaces
{
    public interface IMessageRepository
    {
        Task SaveMessage(Message message);
        Task DeleteMessage(Message message);
        Task<Message> GetMessageById(int id);
        Task<IEnumerable<Message>> GetAllRoomMessages(int roomId);
        Task<IEnumerable<Message>> GetLastMessages(int count, int roomId);
    }
}
