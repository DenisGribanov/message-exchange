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
        private readonly IDataStore _dataStore;

        public ExchangeController(ILogger<ExchangeController> logger,
                IExchangeService exchangeService,
                WebSocketConnectionManager webSocketConnectionManager,
                IDataStore dataStore)
        {
            _logger = logger;
            _exchangeService = exchangeService;
            _webSocketConnectionManager = webSocketConnectionManager;
            _dataStore = dataStore;
        }

        /// <summary>
        /// ��������� ���������
        /// </summary>
        /// <param name="message">����� ���������</param>
        /// <param name="number">���������� ����� ���������</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] string message, long number)
        {
            await _exchangeService.SaveMessage(new Domain.Models.MessageModel { Text = message, Number = number, DateTimeCreated = DateTime.UtcNow });
            return Ok();
        }

        /// <summary>
        /// ws ����������� � ������
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

                _logger.LogInformation("����� WS ���������� {guid}", wsGuid);

                var buffer = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult? received = null;
                received = await webSocket.ReceiveAsync(buffer, cancellationToken);

                if (received != null && received.CloseStatus != null)
                {
                    _logger.LogInformation("WS ���������� {guid} �������", wsGuid);
                    _webSocketConnectionManager.RemoveSocket(wsGuid);
                }
            }
            else
            {
                _logger.LogWarning("������ ����������� �� WS");
            }
        }

        /// <summary>
        /// �������� ������ �� ��������� ���������
        /// </summary>
        /// <returns></returns>
        [HttpGet("last")]
        public async Task<IActionResult> GetLast()
        {
            var l = await _dataStore.GetLastMessages();
            return Ok(l);
        }
    }
}
