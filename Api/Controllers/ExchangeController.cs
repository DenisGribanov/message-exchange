using Domain.Abstractions;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {

        private readonly ILogger<ExchangeController> _logger;
        private readonly IExchangeService _exchangeService;
        private readonly IWsConnectionManager _webSocketConnectionManager;
        private readonly IDataStore _dataStore;

        public ExchangeController(ILogger<ExchangeController> logger,
                IExchangeService exchangeService,
                IWsConnectionManager webSocketConnectionManager,
                IDataStore dataStore)
        {
            _logger = logger;
            _exchangeService = exchangeService;
            _webSocketConnectionManager = webSocketConnectionManager;
            _dataStore = dataStore;
        }

        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message">текст сообщения</param>
        /// <param name="number">порядковый номер сообщения</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] [MaxLength(128)] string message, [Required]long number, CancellationToken cancellationToken = default)
        {
            await _exchangeService.SaveMessage(new MessageModel { Text = message, Number = number, DateTimeCreated = DateTime.UtcNow }, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// ws подулючение к обмену
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("ws")]
        public async Task Ws(CancellationToken cancellationToken)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var wsGuid = _webSocketConnectionManager.AddSocket(webSocket);

                _logger.LogInformation("Новое WS соединение {guid}", wsGuid);

                var buffer = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult? received = await webSocket.ReceiveAsync(buffer, cancellationToken);

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

        /// <summary>
        /// показать список из последних сообщений
        /// </summary>
        /// <returns></returns>
        [HttpGet("last")]
        public async Task<IActionResult> GetLast(CancellationToken cancellationToken = default)
        {
            var result = await _exchangeService.GetLastMessages(cancellationToken);
            return Ok(result);
        }
    }
}
