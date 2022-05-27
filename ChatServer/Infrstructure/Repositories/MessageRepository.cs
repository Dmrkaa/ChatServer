using ChatServer.Domain.Entities;
using ChatServer.Domain.Repository_Interfaces;
using ChatServer.Infrstructure.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace ChatServer.Infrstructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDataContext _chatDataContext;

        public MessageRepository(ChatDataContext chatDataContext, IMemoryCache memoryCache)
        {
            _chatDataContext = chatDataContext;
        }
        public Task DeleteMessage(Message message)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Message>> GetAllRoomMessages(int roomId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Message>> GetLastMessages(int count, int roomId)
        {
            return await _chatDataContext.Messages.Where(x => x.Room.Id == roomId).OrderByDescending<Message, DateTime>(u => u.TimeStamp).Take(count).ToListAsync();

        }

        public Task<Message> GetMessageById(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task SaveMessage(Message message)
        {
            var newMessage = await _chatDataContext.Messages.AddAsync(message);
            await _chatDataContext.SaveChangesAsync();
        }
    }
}
