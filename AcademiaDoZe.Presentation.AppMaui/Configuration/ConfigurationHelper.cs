
// Amanda Soares Poroski

using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) =
            ObterConfiguracaoAtual();

        var repositoryConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        services.AddSingleton(repositoryConfig);

        WeakReferenceMessenger.Default.Register<
            RepositoryConfig,
            BancoPreferencesUpdatedMessage>(
            repositoryConfig,
            (r, m) =>
            {
                var (novaConnectionString, novoDatabaseType) =
                    ObterConfiguracaoAtual();

                r.ConnectionString = novaConnectionString;
                r.DatabaseType = novoDatabaseType.ToInfrastructure();
            });

        services.AddApplicationServices();
    }

    public static (
        string ConnectionString,
        AppDatabaseType DatabaseType)
        ObterConfiguracaoAtual()
    {
        var databaseTypeStr = Preferences.Get(
            "DatabaseType",
            AppDatabaseType.Sqlite.ToString());

        if (!Enum.TryParse<AppDatabaseType>(
                databaseTypeStr,
                out var databaseType))
        {
            databaseType = AppDatabaseType.Sqlite;
        }

        string connectionString;

        if (databaseType == AppDatabaseType.Sqlite)
        {
            // Pasta de dados do aplicativo MAUI
            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "db_academia_do_ze.db");

            // Garantir que a pasta exista
            var pasta = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrWhiteSpace(pasta))
            {
                Directory.CreateDirectory(pasta);
            }

            var complemento = Preferences.Get(
                "Sqlite_Complemento",
                "Default Timeout=5;");

            connectionString =
                $"Data Source={dbPath};{complemento}";
        }
        else
        {
            var prefix =
                databaseType == AppDatabaseType.SqlServer
                    ? "SqlServer"
                    : "MySql";

            var defaultServer =
                databaseType == AppDatabaseType.SqlServer
                    ? "172.24.32.1"
                    : "10.30.21.16";

            var defaultUser =
                databaseType == AppDatabaseType.SqlServer
                    ? "sa"
                    : "root";

            var defaultComplemento =
                databaseType == AppDatabaseType.SqlServer
                    ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"
                    : "Connection Timeout=5;Default Command Timeout=30;";

            var dbServer = Preferences.Get(
                $"{prefix}_Servidor",
                Preferences.Get(
                    "Servidor",
                    defaultServer));

            var dbDatabase = Preferences.Get(
                $"{prefix}_Banco",
                Preferences.Get(
                    "Banco",
                    "db_academia_do_ze"));

            var dbUser = Preferences.Get(
                $"{prefix}_Usuario",
                Preferences.Get(
                    "Usuario",
                    defaultUser));

            var dbPassword = Preferences.Get(
                $"{prefix}_Senha",
                Preferences.Get(
                    "Senha",
                    "abcBolinhas12345"));

            var dbComplemento = Preferences.Get(
                $"{prefix}_Complemento",
                Preferences.Get(
                    "Complemento",
                    defaultComplemento));

            connectionString =
                $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        return (
            connectionString,
            databaseType);
    }
}
