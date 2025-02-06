using Domain.Abstractions;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {

        private readonly ILogger<ExchangeController> _logger;
        private readonly IExchangeService _exchangeService;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        public ExchangeController(ILogger<ExchangeController> logger, IExchangeService exchangeService, WebSocketConnectionManager webSocketConnectionManager)
        {
            _logger = logger;
            _exchangeService = exchangeService;
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] string message, long number)
        {
            await _exchangeService.SaveMessage(new Domain.Models.MessageModel { Text = message, Number = number, DateTimeCreated = DateTime.UtcNow });
            return Ok();
        }

        [HttpGet("ws")]
        public async Task Ws(CancellationToken cancellationToken)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var wsGuid = _webSocketConnectionManager.AddSocket(webSocket);

                _logger.LogInformation("Новое WS соединение {guid}", wsGuid);

                var buffer = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult? received = null;
                received = await webSocket.ReceiveAsync(buffer, cancellationToken);

                if (received != null && received.CloseStatus != null)
                {
                    _logger.LogInformation("WS соединение {guid} закрыто", wsGuid);
                    _webSocketConnectionManager.RemoveSocket(wsGuid);
                }
            }
            else
            {
                _logger.LogWarning("Данное подключение не WS");
            }
        }
    }
}
