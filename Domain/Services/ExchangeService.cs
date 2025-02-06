using Domain.Abstractions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Net.Sockets;

namespace Domain.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IDataStore _dataStore;
        private readonly ILogger<ExchangeService> _logger;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        public ExchangeService(IDataStore dataStore, ILogger<ExchangeService> logger, WebSocketConnectionManager webSocketConnectionManager)
        {
            _dataStore = dataStore;
            _logger = logger;
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        public async Task SaveMessage(MessageModel message)
        {
            await _dataStore.SaveMessage(message);

            foreach(var socket in _webSocketConnectionManager.GetAllSockets())
            {
                if (socket.Value.State == WebSocketState.Open)
                {
                    await socket.Value.SendAsync(System.Text.Encoding.UTF8.GetBytes(message.Text), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("WS клиенту {guid} передано сообщение {@model}",socket.Key, message);
                }
            }
        }
    }
}
