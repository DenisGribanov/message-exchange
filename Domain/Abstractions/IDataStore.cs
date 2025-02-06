using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstractions
{
    public interface IDataStore
    {
        Task SaveMessage(MessageModel store);

        Task<List<MessageModel>> GetLastMessages();
    }
}
