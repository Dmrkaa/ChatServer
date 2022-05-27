using ChatServer.Application.Services;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Repository_Interfaces;
using ChatServer.SharedModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatServer.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly IMessageRepository _msgRepository;
        private readonly IRoomRepository _roomRepository;
        private static ConcurrentDictionary<string, UserModel> _roomMapping = new ConcurrentDictionary<string, UserModel>();
        public ChatHub(IMessageRepository msgRepository, IRoomRepository roomRepository) : base()
        {
            _msgRepository = msgRepository;
            _roomRepository = roomRepository;
        }
        public override async Task OnConnectedAsync()
        {
            var name = Context.User.Identity.Name;
            await base.OnConnectedAsync();
            await InitNewUserAsync(name);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _roomMapping.TryRemove(Context.User.Identity.Name, out var room);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Name);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(MessageModel message)
        {
            string userName = message.UserName;
            ChatMessageBuilder msgBuilder = new ChatMessageBuilder(message.Message, _roomMapping[userName]);
            await _msgRepository.SaveMessage(msgBuilder.BuildMessage());

            await Clients.GroupExcept(_roomMapping[userName].RoomName, Context.ConnectionId).SendAsync("RecieveMessageHandler", message);
        }

        private async Task InitNewUserAsync(string userName)
        {
            Room room = await _roomRepository.GetRoomByUserName(userName);
            if (room != null)
            {
                var user = room.Users.Where(x => x.UserName == userName).FirstOrDefault();
                UserModel userModel = new UserModel() { Id = user.Id, Name = user.UserName, RoomId = room.Id, RoomName = room.Name };

                _roomMapping.TryAdd(userName, userModel);
                await Groups.AddToGroupAsync(Context.ConnectionId, userModel.RoomName);
                var history = await _msgRepository.GetLastMessages(50, userModel.RoomId);
                if (history != null)
                    await Clients.Caller.SendAsync("RecieveHistoryHandler", history.Select(x => x.AsDto()));
            }
        }
    }
}
