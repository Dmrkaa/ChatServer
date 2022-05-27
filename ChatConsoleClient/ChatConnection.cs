using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Connections;
using ChatServer.SharedModels;

namespace ChatConsoleClient
{
    public class ChatConnection
    {
        public LoginModel loginModel;
        string ConnectionString { get; init; }
        public HubConnection connection { get; set; }
        private string tokenString { get; init; }
        public ChatConnection(LoginModel login, string connectionString, string token)
        {
            this.loginModel = login;
            this.ConnectionString = connectionString;
            this.tokenString = token;
        }

        private void InitHubConnection()
        {
            connection = new HubConnectionBuilder()
            .WithUrl((ConnectionString + "/?access_token=" + tokenString)) //options.AccessTokenProvider is not working from console idk
                .WithAutomaticReconnect()
                .Build();


            connection.On<IEnumerable<MessageModel>>("RecieveHistoryHandler",
                (history) =>
                {
                    foreach (var msg in history.OrderBy(x => x.TimeStamp))
                    {
                        Console.WriteLine(msg);
                    }
                });

            connection.On<MessageModel>("RecieveMessageHandler",
                (msg) =>
                {
                    Console.WriteLine(msg);
                });
        }

        public void Connect()
        {
            InitHubConnection();
            connection.StartAsync().Wait();
        }
    }
}
