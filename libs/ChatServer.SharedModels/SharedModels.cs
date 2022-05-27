using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.SharedModels
{
    public class RoomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class UserModel
    {
        public string Id { get; set; }
        public int RoomId { get; set; }
        public string Name { get; set; }
        public string RoomName { get; set; }
    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RoomName { get; set; }
        public LoginModel()
        {

        }
        public LoginModel(string userName, string password, string roomName)
        {
            UserName = userName;
            Password = password;
            RoomName = roomName;
        }

    }

    public class MessageModel
    {
        public MessageModel()
        {
        }

        public MessageModel(string message, string userName, DateTime timeStamp)
        {
            Message = message;
            UserName = userName;
            TimeStamp = timeStamp;
        }

        public string Message { get; set; }
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return $"{TimeStamp} {UserName}: {Message}";
        }
    }

}
