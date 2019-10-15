using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FlashCards.Data
{
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("flash-cards");
        }

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}