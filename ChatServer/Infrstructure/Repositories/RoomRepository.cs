using ChatServer.Domain.Entities;
using ChatServer.Domain.Repository_Interfaces;
using ChatServer.Infrstructure.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace ChatServer.Infrstructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ChatDataContext _chatDataContext;
        private IMemoryCache cache;
        public RoomRepository(ChatDataContext chatDataContext, IMemoryCache memoryCache)
        {
            _chatDataContext = chatDataContext;
            cache = memoryCache;
        }
        public async Task<Room> CreateRoom(string roomName)
        {
            var newRoom = new Room() { Name = roomName };
            var createdRoom = await _chatDataContext.Rooms.AddAsync(newRoom);
            int n = await _chatDataContext.SaveChangesAsync();

            if (n > 0)
            {
                cache.Set(createdRoom.Entity.Name, createdRoom.Entity, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                });
            }
            return createdRoom.Entity;

        }

        public async Task DeleteRoom(int roomId)
        {
            var room = await _chatDataContext.Rooms.FindAsync(roomId);
            _chatDataContext.Remove(room);
            await _chatDataContext.SaveChangesAsync();
        }


        public async Task<Room> GetRoom(int roomId)
        {
            Room room = null;
            if (!cache.TryGetValue(roomId, out room))
            {
                room = await _chatDataContext.Rooms.Include(x => x.Users).FirstOrDefaultAsync(r => r.Id == roomId);
                if (room != null)
                {
                    cache.Set(room.Id, room,
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
                }
            }
            return room;
        }

        public async Task<Room> GetRoom(string roomName)
        {
            var room = await _chatDataContext.Rooms.FirstOrDefaultAsync(r => r.Name == roomName);
            return room;
        }

        public async Task<Room> GetRoomByUserName(string userName)
        {
            return await _chatDataContext.Rooms.Include(x => x.Users).Where(x => x.Users.Where(u => u.UserName == userName).FirstOrDefault().UserName == userName).FirstOrDefaultAsync();
        }
    }
}
