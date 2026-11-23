
// Amanda Soares Poroski

using AcademiaDoZe.Infrastructure.Exceptions;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool>
        _bancosInicializados = new();

    private static readonly SemaphoreSlim
        _semaforoInicializacao = new(1, 1);

    public static async Task InicializarAsync(
        string connectionString,
        DatabaseType databaseType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var key = $"{databaseType}:{connectionString}";

        if (_bancosInicializados.ContainsKey(key))
            return;

        await _semaforoInicializacao.WaitAsync(
            cancellationToken);

        try
        {
            if (_bancosInicializados.ContainsKey(key))
                return;

            var scriptSql = ObterScript(databaseType);

            await using var connection =
                DbProvider.CreateConnection(
                    connectionString,
                    databaseType);

            await connection.OpenAsync(cancellationToken);

            // Inicializa as tabelas existentes
            await using (var command =
                DbProvider.CreateCommand(
                    scriptSql,
                    connection))
            {
                await command.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            // Avaliacao 03:
            // Atualiza somente o banco SQLite.
            if (databaseType == DatabaseType.Sqlite)
            {
                await MigrarAcessoSqliteAsync(
                    connection,
                    cancellationToken);
            }

            // Marca como inicializado somente
            // depois de concluir todas as etapas.
            _bancosInicializados.TryAdd(key, true);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_INICIALIZAR_BANCO",
                $"Erro ao inicializar banco de dados: {ex.Message}",
                ex);
        }
        finally
        {
            _semaforoInicializacao.Release();
        }
    }

    // =====================================================
    // AVALIACAO 03 - MIGRACAO DA TABELA DE ACESSOS
    // =====================================================

    private static async Task MigrarAcessoSqliteAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        // Verifica se a tabela de acessos existe.
        await using (var verificarTabela =
            DbProvider.CreateCommand(
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = 'tb_acesso';
                """,
                connection))
        {
            var resultado =
                await verificarTabela.ExecuteScalarAsync(
                    cancellationToken);

            if (Convert.ToInt32(resultado) == 0)
            {
                throw new InvalidOperationException(
                    "A tabela tb_acesso não foi encontrada.");
            }
        }

        // Verifica as colunas existentes.
        bool colunaSaidaExiste = false;

        await using (var verificarColunas =
            DbProvider.CreateCommand(
                "PRAGMA table_info(tb_acesso);",
                connection))
        {
            await using var reader =
                await verificarColunas.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(
                cancellationToken))
            {
                var nomeColuna =
                    reader.GetString(1);

                if (string.Equals(
                    nomeColuna,
                    "data_hora_saida",
                    StringComparison.OrdinalIgnoreCase))
                {
                    colunaSaidaExiste = true;
                    break;
                }
            }
        }

        // Adiciona a coluna somente se estiver faltando.
        if (!colunaSaidaExiste)
        {
            await using var adicionarColuna =
                DbProvider.CreateCommand(
                    """
                    ALTER TABLE tb_acesso
                    ADD COLUMN data_hora_saida TEXT NULL;
                    """,
                    connection);

            await adicionarColuna.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }

    // =====================================================
    // SCRIPT SQL EMBARCADO
    // =====================================================

    public static string ObterScript(
        DatabaseType databaseType)
    {
        var nomeScript =
            DbProvider.GetScriptName(databaseType);

        var assembly =
            Assembly.GetExecutingAssembly();

        var recursos =
            assembly.GetManifestResourceNames();

        var resourceName =
            recursos.FirstOrDefault(r =>
                r.EndsWith(
                    nomeScript,
                    StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InfrastructureException(
                "SCRIPT_EMBARCADO_NAO_ENCONTRADO",
                $"Script SQL embarcado '{nomeScript}' não encontrado.");
        }

        using var stream =
            assembly.GetManifestResourceStream(
                resourceName)
            ?? throw new InfrastructureException(
                "SCRIPT_EMBARCADO_NAO_ENCONTRADO",
                $"Script SQL embarcado '{nomeScript}' não encontrado.");

        using var reader =
            new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
