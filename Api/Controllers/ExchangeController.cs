using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {

        private readonly ILogger<ExchangeController> _logger;
        private readonly IExchangeService _exchangeService;
        private readonly IWsConnectionManager _webSocketConnectionManager;

        public ExchangeController(ILogger<ExchangeController> logger,
                IExchangeService exchangeService,
                IWsConnectionManager webSocketConnectionManager)
        {
            _logger = logger;
            _exchangeService = exchangeService;
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        /// <summary>
        /// ��������� ���������
        /// </summary>
        /// <param name="message">����� ���������</param>
        /// <param name="number">���������� ����� ���������</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody][MaxLength(128)] string message, [Required] long number, CancellationToken cancellationToken = default)
        {
            await _exchangeService.SaveMessage(new MessageModel { Text = message, Number = number, DateTimeCreated = DateTime.UtcNow }, cancellationToken);
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
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("������ ����������� �� WS");
                return;
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var wsGuid = _webSocketConnectionManager.AddSocket(webSocket);

            _logger.LogInformation("����� WS ���������� {guid}", wsGuid);

            var buffer = new ArraySegment<byte>(new byte[4096]);
            WebSocketReceiveResult? received = await webSocket.ReceiveAsync(buffer, cancellationToken);

            if (received != null && received.CloseStatus != null)
            {
                _logger.LogInformation("WS ���������� {guid} �������", wsGuid);
                _webSocketConnectionManager.RemoveSocket(wsGuid);
            }
        }

        [HttpGet]
        /// <summary>
        /// �������� ������ �� ��������� ���������
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetMessages([FromQuery] DateTime dateStart, DateTime dateEnd, CancellationToken cancellationToken = default)
        {
            var result = await _exchangeService.GetMessages(dateStart, dateEnd, cancellationToken);
            return Ok(result);
        }
    }
}
