using AutoMapper;
using Domain.Abstractions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace Domain.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IDataStore _dataStore;
        private readonly ILogger<ExchangeService> _logger;
        private readonly IWsConnectionManager _webSocketConnectionManager;
        private readonly IMapper _mapper;

        public ExchangeService(IDataStore dataStore, ILogger<ExchangeService> logger, 
                IWsConnectionManager webSocketConnectionManager, 
                IMapper mapper)
        {
            _dataStore = dataStore;
            _logger = logger;
            _webSocketConnectionManager = webSocketConnectionManager;
            _mapper = mapper;
        }

        public async Task<List<MessageModelResponse>> GetLastMessages()
        {
            var list = await _dataStore.GetLastMessages();

            return _mapper.Map<List<MessageModelResponse>>(list);
        }

        public async Task SaveMessage(MessageModel message)
        {
            await _dataStore.SaveMessage(message);

            var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);
            var jsonByte = System.Text.Encoding.UTF8.GetBytes(jsonMsg);

            foreach (var socket in _webSocketConnectionManager.GetAllSockets())
            {
                if (socket.Value.State == WebSocketState.Open)
                {
                    await socket.Value.SendAsync(jsonByte, WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("WS клиенту {guid} передано сообщение {@model}", socket.Key, message);
                }
            }
        }
    }
}
