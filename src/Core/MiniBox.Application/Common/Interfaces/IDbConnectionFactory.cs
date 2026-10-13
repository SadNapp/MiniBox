using System.Data.Common;

namespace MiniBox.Application.Common.Interfaces;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
