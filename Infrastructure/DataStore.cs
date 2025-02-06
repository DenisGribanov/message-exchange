using Domain.Abstractions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DataStore : IDataStore
    {
        private readonly ILogger<DataStore> _logger;

        public DataStore(ILogger<DataStore> logger)
        {
            _logger = logger;
        }

        public async Task<List<MessageModel>> GetMessages()
        {
            _logger.LogInformation("GetMessages");
            return new List<MessageModel>();
        }

        public async Task SaveMessage(MessageModel message)
        {
            _logger.LogInformation("{@model}", message);
        }
    }
}
