using AutoMapper;

using ChatServer.Domain.Entities;
using ChatServer.SharedModels;

namespace ChatServer.Application.Services
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<ChatUser, LoginModel>().ReverseMap();
        }
    }
}
