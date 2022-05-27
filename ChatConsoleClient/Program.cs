using ChatServer.SharedModels;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatConsoleClient
{
    internal class Program
    {
        public static HttpClient client = new HttpClient();

        private static string jwtToken = "";
        private static string ip;
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter IP (localhost:81 for nginx in docker-comsope)");
                ip = Console.ReadLine();

                Console.WriteLine("Type L for Login or R for registration");
                string LoginOrRegistrationAction = Console.ReadLine();

                Console.WriteLine("Enter User Name: ");
                string userName = Console.ReadLine();
                Console.WriteLine("Enter Room Name: ");
                string roomName = Console.ReadLine();
                LoginModel login = new LoginModel() { UserName = userName, Password = "AnyPassword", RoomName = roomName };
                int loginResult = 0;
                InitChatApiConnection();
                switch (LoginOrRegistrationAction)
                {
                    case "L":
                        loginResult = TryToLogin(login);
                        break;
                    case "R":
                        var registrationResult = await Registration(login);
                        if (registrationResult == 202)
                        {
                            loginResult = TryToLogin(login);
                        }
                        else
                        {
                            throw new Exception("Something went wrong");
                        }
                        break;
                }


                if (loginResult == (int)System.Net.HttpStatusCode.Accepted)
                {
                    var _bearer_token = jwtToken.Trim(new[] { '{', '}', '"', '"' });
                    ChatConnection conn = new ChatConnection(login, $"http://{ip}/chathub", _bearer_token);
                    conn.Connect();
                    StartChat(conn);
                    await conn.connection.StopAsync();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
            finally
            {
                client.Dispose();
            }
        }

        static void StartChat(ChatConnection conn)
        {
            Console.WriteLine($"Joined to {conn.loginModel.RoomName}");
            Console.WriteLine($"Type >Exit to leave.");
            for (int i = 0; i < 100; i++)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    if (message == ">Exit")
                    {
                        break;
                    }
                    conn.connection.InvokeCoreAsync("SendMessage", args: new[] { new MessageModel(message, conn.loginModel.UserName, DateTime.Now) });
                }
            }
        }

        static async Task<int> Registration(LoginModel login)
        {
            string uri = client.BaseAddress + "register";
            ByteArrayContent byteArray = LoginToByteArray(login);
            byteArray.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync(uri, byteArray);

            Console.WriteLine($"Register result:{resp.StatusCode}");
            return (int)resp.StatusCode;
        }

        static void InitChatApiConnection()
        {
            client.BaseAddress = new Uri($"http://{ip}/api/Account/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }



        static int TryToLogin(LoginModel login)
        {
            string uri = client.BaseAddress + "login";
            ByteArrayContent byteArray = LoginToByteArray(login);
            byteArray.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = client.PostAsync(uri, byteArray);  // Blocking call!
            var rez = resp.Result.Content.ReadAsStringAsync();
            jwtToken = rez.Result.Split(':')[1];
            Console.WriteLine($"Login result:{resp.Result.StatusCode}");
            return (int)resp.Result.StatusCode;
        }

        static ByteArrayContent LoginToByteArray(LoginModel login)
        {
            var json = JsonConvert.SerializeObject(login); // or JsonSerializer.Serialize if using System.Text.Json
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            return byteContent;
        }
    }
}
