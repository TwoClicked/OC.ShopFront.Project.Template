using Microsoft.AspNetCore.SignalR.Client;
using OC.LUAC.UiLayer.DTO.Chat;
using System.Net.Http.Json;

namespace OC.LUAC.UiLayer.Services
{
    public class ChatClientService
    {
        private readonly string _hubUrl;
        private readonly IHttpClientFactory _httpClientFactory;
        private HubConnection? _connection;

        // Events
        public event Action<ChatMessageDto>? OnMessageReceived;
        public event Action<int>? OnSessionClosed;

        public ChatClientService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            var apiBaseUrl = config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl missing");

            // SignalR hub does NOT go through ApiClient (no /api/)
            _hubUrl = $"{apiBaseUrl}/chathub";

            _httpClientFactory = httpClientFactory;
        }

        public async Task ConnectAsync(string token)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token)!;
                })
                .WithAutomaticReconnect()
                .Build();

            // Handle incoming messages
            _connection.On<ChatMessageDto>("ReceiveMessage", msg =>
            {
                OnMessageReceived?.Invoke(msg);
            });

            // Handle session closed
            _connection.On<int>("SessionClosed", closedSessionId =>
            {
                OnSessionClosed?.Invoke(closedSessionId);
            });

            _connection.Closed += async (error) =>
            {
                Console.WriteLine($"⚠️ SignalR connection closed: {error?.Message}");
                await Task.Delay(2000);
                try
                {
                    await _connection.StartAsync();
                    Console.WriteLine("🔄 SignalR reconnected.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Reconnect failed: {ex.Message}");
                }
            };

            await _connection.StartAsync();
            Console.WriteLine("✅ SignalR connected!");
        }

        public async Task<int> StartSessionAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // ✅ Do NOT prepend /api (ApiClient already has it)
            var session = await client.GetFromJsonAsync<ChatSessionDto>("chat/sessions/me");

            if (session == null)
                throw new InvalidOperationException("Unable to start or fetch chat session.");

            return session.Id;
        }

        public async Task<List<ChatMessageDto>> GetMyMessagesAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var messages = await client.GetFromJsonAsync<List<ChatMessageDto>>(
                "chat/sessions/me/messages");

            return messages ?? new List<ChatMessageDto>();
        }

        public async Task SendMessageAsync(int sessionId, string message)
        {
            if (_connection == null)
                throw new InvalidOperationException("Not connected to chat hub");

            await _connection.InvokeAsync("SendMessage", sessionId, message);
        }

        public async Task JoinSessionAsync(int sessionId)
        {
            if (_connection == null)
                throw new InvalidOperationException("Not connected to chat hub");

            await _connection.InvokeAsync("JoinSession", sessionId);
        }

        /// <summary>
        /// Close a chat session (Admin only).
        /// </summary>
        public async Task CloseSessionAsync(int sessionId, string token)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"chat/sessions/{sessionId}/close", null);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to close session {sessionId}. Status: {response.StatusCode}");
            }
        }
    }

}
