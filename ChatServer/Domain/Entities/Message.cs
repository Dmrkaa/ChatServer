using System;

namespace ChatServer.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int RoomId { get; set; } //FK
        public string ChatUserId { get; set; }//FK
        public DateTime TimeStamp { get; set; }
        public string Text { get; set; }
        public virtual Room Room { get; set; }//Navigation prop
        public virtual ChatUser User { get; set; }//Navigation prop
    }
}
