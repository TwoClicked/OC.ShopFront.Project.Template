
using Microsoft.AspNetCore.SignalR.Client;

namespace OC.LUAC.UiLayer.Services
{
    public class ChatClientService : IAsyncDisposable
    {


        private readonly HubConnection _connection;

        public ChatClientService(IConfiguration config)
        {

            
        }


        public async ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
