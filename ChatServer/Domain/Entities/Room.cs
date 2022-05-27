using System.Collections.Generic;

namespace ChatServer.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<ChatUser> Users { get; set; }
    }
}
