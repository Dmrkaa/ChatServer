using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ChatServer.Domain.Entities
{
    public class ChatUser : IdentityUser
    {
        public int RoomId { get; set; } //FK
        public virtual Room Room { get; set; } //Navigation prop
        public virtual ICollection<Message> Messages { get; set; }
    }
}
