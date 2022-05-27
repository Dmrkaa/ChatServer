using ChatServer.Domain.Entities;
using ChatServer.SharedModels;
using System;

namespace ChatServer.Application.Services
{
    public abstract class MessageBuilder
    {
        public Message Message { get; private set; }
        public void CreateMessage()
        {
            Message = new Message();
        }
        public abstract void SetText(string text);
        public abstract void SetRoomId(int roomId);
        public abstract void SetUserId(string userId);

        public Message BuildMessage()
        {
            return this.Message;
        }

    }
    public class ChatMessageBuilder : MessageBuilder
    {
        public ChatMessageBuilder(string text, UserModel userModel)
        {
            CreateMessage();
            SetText(text);
            SetRoomId(userModel.RoomId);
            SetUserId(userModel.Id);
            SetCurrentTimeStamp();
        }
        public void SetCurrentTimeStamp()
        {
            this.Message.TimeStamp = DateTime.Now;
        }
        public override void SetRoomId(int roomId)
        {
            this.Message.RoomId = roomId;
        }

        public override void SetText(string text)
        {
            this.Message.Text = text;
        }

        public override void SetUserId(string userId)
        {
            this.Message.ChatUserId = userId;
        }
    }
}
