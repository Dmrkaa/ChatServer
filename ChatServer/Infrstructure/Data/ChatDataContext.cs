using ChatServer.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChatServer.Infrstructure.Data
{
    public class ChatDataContext : IdentityDbContext<ChatUser>
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ChatDataContext(DbContextOptions<ChatDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
