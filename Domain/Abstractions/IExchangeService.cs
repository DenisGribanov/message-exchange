using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstractions
{
    public interface IExchangeService
    {
        Task<List<MessageModelResponse>> GetLastMessages(CancellationToken cancellationToken = default);
        Task SaveMessage(MessageModel message, CancellationToken cancellationToken = default);
    }
}
