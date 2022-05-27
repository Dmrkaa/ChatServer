using ChatServer.SharedModels;
using ChatServer.Domain.Entities;

namespace ChatServer.Application.Services
{
    public static class ModelExtensions
    {
        public static MessageModel AsDto(this Message message)
        {
            return new MessageModel()
            {
                Message = message.Text,
                TimeStamp = message.TimeStamp,
                UserName = message.User.UserName
            };
        }
        public static RoomModel AsDto(this Room room)
        {
            return new RoomModel
            {
                Id = room.Id,
                Name = room.Name
            };
        }
    }


}
