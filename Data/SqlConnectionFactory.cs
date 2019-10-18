using System;
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
            var databaseUrl =
#if DEBUG
                configuration.GetValue<string>("flash-cards-database-url");
#endif
#if !DEBUG
            configuration.GetValue<string>("DATABASE_URL");
#endif
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
#if DEBUG
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
#endif
            };

            _connectionString = builder.ToString();
        }

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}