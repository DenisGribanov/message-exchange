using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstractions
{
    public interface IWsConnectionManager
    {
        Guid AddSocket(WebSocket socket);

        WebSocket? GetSocket(Guid socketId);

        ConcurrentDictionary<Guid, WebSocket> GetAllSockets();

        Guid? GetSocketId(WebSocket socket);

        void RemoveSocket(Guid socketId);
    }
}
