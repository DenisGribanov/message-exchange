using AutoMapper;
using Domain.Abstractions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

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

        public async Task<List<MessageModelResponse>> GetMessages(DateTime dateStart, DateTime dateEnd, CancellationToken cancellationToken = default)
        {
            var list = await _dataStore.GetMessages(dateStart, dateEnd);

            return _mapper.Map<List<MessageModelResponse>>(list);
        }

        public async Task SaveMessage(MessageModel message, CancellationToken cancellationToken = default)
        {
            await _dataStore.SaveMessage(message);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message, options);
            var jsonByte = System.Text.Encoding.UTF8.GetBytes(jsonMsg)!;
            
            await Task.WhenAll(SendToWs(jsonByte, jsonMsg));
        }

        private IEnumerable<Task> SendToWs(byte[] jsonByte, string message)
        {
            foreach (var socket in _webSocketConnectionManager.GetAllSockets())
            {
                if (socket.Value.State == WebSocketState.Open)
                {
                    _logger.LogInformation("WS клиенту {guid} передано сообщение {@model}", socket.Key, message);
                    yield return socket.Value.SendAsync(jsonByte, WebSocketMessageType.Text, true, default);
                }
            }
        }
    }
}
