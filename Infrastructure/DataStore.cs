using Domain.Abstractions;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Text;

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

        public async Task<List<MessageModel>> GetMessages(DateTime dateStart, DateTime dateEnd)
        {
            List<MessageModel> result = new List<MessageModel>();

            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();

            cmd.Connection = connection;
            cmd.CommandText = "SELECT \"Id\", \"Number\", \"Text\", \"DateCreated\" FROM public.\"Messages\" WHERE \"DateCreated\" >= @dateStart AND \"DateCreated\" <= @dateEnd";
            cmd.Parameters.AddWithValue("dateStart", NpgsqlTypes.NpgsqlDbType.TimestampTz, dateStart);
            cmd.Parameters.AddWithValue("dateEnd", NpgsqlTypes.NpgsqlDbType.TimestampTz, dateEnd);
            cmd.CommandType = System.Data.CommandType.Text;

            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                while (dataReader.Read())
                {
                    result.Add(new MessageModel
                    {
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
            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();

            cmd.Connection = connection;
            cmd.CommandText = "INSERT INTO public.\"Messages\" (\"Number\", \"Text\", \"DateCreated\") VALUES (@num, @text, @date)\r\n   RETURNING \"Id\"";
            cmd.Parameters.AddWithValue("num", message.Number);
            cmd.Parameters.AddWithValue("text", message.Text);
            cmd.Parameters.AddWithValue("date", message.DateTimeCreated);

            cmd.CommandType = System.Data.CommandType.Text;

            long id = default;

            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                while (dataReader.Read())
                {
                    id = dataReader.GetInt64(0);
                }
            }

            connection.Dispose();
            cmd.Dispose();

            _logger.LogInformation("Добавлена запись в БД id - {@Id}", id);
        }
    }
}
