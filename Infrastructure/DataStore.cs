using Domain.Abstractions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class DataStore : IDataStore
    {
        private readonly ILogger<DataStore> _logger;
        private readonly string _connectionString;

        public DataStore(ILogger<DataStore> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("Connection");
        }

        public async Task<List<MessageModel>> GetLastMessages()
        {
            List<MessageModel> result = new List<MessageModel>();

            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();

            cmd.Connection = connection;
            cmd.CommandText = "SELECT \"Id\", \"Number\", \"Text\", \"DateCreated\" FROM public.\"Messages\" WHERE \"DateCreated\" > now() - interval '10 minute'";
            cmd.CommandType = System.Data.CommandType.Text;
            
            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                while (dataReader.Read())
                {
                    result.Add(new MessageModel
                    {
                        Id = dataReader.GetInt64("Id"),
                        Number = dataReader.GetInt64("Number"),
                        Text = dataReader.GetString("Text"),
                        DateTimeCreated = dataReader.GetDateTime("DateCreated")
                    });
                }
            }

            connection.Dispose();
            cmd.Dispose();

            _logger.LogInformation("GetLastMessages - {0} строк", result.Count);

            return result;
        }

        public async Task SaveMessage(MessageModel message)
        {
            _logger.LogInformation("{@model}", message);
        }
    }
}
