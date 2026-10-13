using System.Data.Common;
using MiniBox.Application.Common.Interfaces;
using Npgsql;

namespace MiniBox.Infrastructure.Persistence;

public sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public DbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
