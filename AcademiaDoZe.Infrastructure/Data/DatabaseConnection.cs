// Amanda Soares Poroski

using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DatabaseConnection
{
    public static DbConnection CreateConnection()
    {
        var connection = DatabaseConfig.Factory.CreateConnection()!;
        connection.ConnectionString = DatabaseConfig.ConnectionString;
        return connection;
    }
}