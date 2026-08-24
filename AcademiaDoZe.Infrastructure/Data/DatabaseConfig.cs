// Amanda Soares Poroski

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DatabaseConfig
{
    public static DatabaseType DatabaseType = DatabaseType.Sqlite;

    public static string ConnectionString =>
        DatabaseType switch
        {
            DatabaseType.SqlServer =>
                "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",

            DatabaseType.MySql =>
                "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",

            DatabaseType.Sqlite =>
                @"Data Source=C:\DEV\AcademiaDoZe\db_academia_do_ze.db",

            _ => throw new Exception("Banco não suportado")
        };

    public static DbProviderFactory Factory =>
        DatabaseType switch
        {
            DatabaseType.SqlServer => SqlClientFactory.Instance,
            DatabaseType.MySql => MySqlClientFactory.Instance,
            DatabaseType.Sqlite => SqliteFactory.Instance,
            _ => throw new Exception("Banco não suportado")
        };
}