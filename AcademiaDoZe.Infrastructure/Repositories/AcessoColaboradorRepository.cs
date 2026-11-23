
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository
    : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
        if (databaseType != DatabaseType.Sqlite)
        {
            throw new NotSupportedException(
                "O repositório de acessos utiliza SQLite.");
        }
    }

    private const string SelectBase = """
        SELECT
            id_acesso,
            pessoa_id,
            data_hora,
            data_hora_saida
        FROM tb_acesso
        WHERE pessoa_tipo = 1
        """;

    private static string FormatarData(DateTime data)
    {
        return data.ToString(
            "yyyy-MM-dd HH:mm:ss.fffffff",
            CultureInfo.InvariantCulture);
    }

    private static DateTime LerData(
        DbDataReader reader,
        int indice)
    {
        return DateTime.Parse(
            Convert.ToString(
                reader.GetValue(indice),
                CultureInfo.InvariantCulture)!,
            CultureInfo.InvariantCulture);
    }

    private static AcessoColaborador Map(
        DbDataReader reader)
    {
        int id = Convert.ToInt32(reader.GetValue(0));
        int colaboradorId = Convert.ToInt32(reader.GetValue(1));

        DateTime entrada = LerData(reader, 2);

        DateTime? saida = reader.IsDBNull(3)
            ? null
            : LerData(reader, 3);

        return AcessoColaborador.Reconstituir(
            id,
            colaboradorId,
            entrada,
            saida);
    }

    public async Task<AcessoColaborador?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            AND id_acesso = @Id;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Id", id, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            return Map(reader);

        return null;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            ORDER BY data_hora DESC, id_acesso DESC;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var acessos = new List<AcessoColaborador>();

        while (await reader.ReadAsync(cancellationToken))
        {
            acessos.Add(Map(reader));
        }

        return acessos;
    }

    public async Task<AcessoColaborador> Adicionar(
        AcessoColaborador entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        const string sql = """
            INSERT INTO tb_acesso
            (
                pessoa_tipo,
                pessoa_id,
                data_hora,
                data_hora_saida
            )
            VALUES
            (
                1,
                @ColaboradorId,
                @Entrada,
                @Saida
            );

            SELECT last_insert_rowid();
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@ColaboradorId",
            entity.ColaboradorId,
            DbType.Int32);

        command.AddParameter(
            "@Entrada",
            FormatarData(entity.DataHoraEntrada),
            DbType.String);

        command.AddParameter(
            "@Saida",
            entity.DataHoraSaida.HasValue
                ? FormatarData(entity.DataHoraSaida.Value)
                : DBNull.Value,
            DbType.String);

        int id = await command.ExecuteScalarIdAsync(
            cancellationToken: cancellationToken);

        return AcessoColaborador.Reconstituir(
            id,
            entity.ColaboradorId,
            entity.DataHoraEntrada,
            entity.DataHoraSaida);
    }

    public async Task<AcessoColaborador> Atualizar(
        AcessoColaborador entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        const string sql = """
            UPDATE tb_acesso
            SET
                data_hora = @Entrada,
                data_hora_saida = @Saida
            WHERE id_acesso = @Id
              AND pessoa_tipo = 1
              AND pessoa_id = @ColaboradorId;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@Id",
            entity.Id,
            DbType.Int32);

        command.AddParameter(
            "@ColaboradorId",
            entity.ColaboradorId,
            DbType.Int32);

        command.AddParameter(
            "@Entrada",
            FormatarData(entity.DataHoraEntrada),
            DbType.String);

        command.AddParameter(
            "@Saida",
            entity.DataHoraSaida.HasValue
                ? FormatarData(entity.DataHoraSaida.Value)
                : DBNull.Value,
            DbType.String);

        int linhas = await command.ExecuteNonQueryAsync(
            cancellationToken);

        if (linhas == 0)
        {
            throw new KeyNotFoundException(
                "Acesso do colaborador não encontrado.");
        }

        return entity;
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM tb_acesso
            WHERE id_acesso = @Id
              AND pessoa_tipo = 1;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Id", id, DbType.Int32);

        int linhas = await command.ExecuteNonQueryAsync(
            cancellationToken);

        return linhas > 0;
    }

    public async Task<IEnumerable<AcessoColaborador>>
        ObterAcessosPorColaboradorPeriodo(
            int? colaboradorId = null,
            DateOnly? inicio = null,
            DateOnly? fim = null,
            CancellationToken cancellationToken = default)
    {
        if (inicio.HasValue &&
            fim.HasValue &&
            inicio.Value > fim.Value)
        {
            throw new ArgumentException(
                "A data inicial não pode superar a final.");
        }

        string sql = $"""
            {SelectBase}
            AND (@ColaboradorId IS NULL
                 OR pessoa_id = @ColaboradorId)
            AND (@Inicio IS NULL
                 OR data_hora >= @Inicio)
            AND (@Fim IS NULL
                 OR data_hora < @Fim)
            ORDER BY data_hora DESC, id_acesso DESC;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@ColaboradorId",
            colaboradorId.HasValue
                ? colaboradorId.Value
                : DBNull.Value,
            DbType.Int32);

        command.AddParameter(
            "@Inicio",
            inicio.HasValue
                ? FormatarData(
                    inicio.Value.ToDateTime(TimeOnly.MinValue))
                : DBNull.Value,
            DbType.String);

        command.AddParameter(
            "@Fim",
            fim.HasValue
                ? FormatarData(
                    fim.Value.AddDays(1)
                        .ToDateTime(TimeOnly.MinValue))
                : DBNull.Value,
            DbType.String);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var acessos = new List<AcessoColaborador>();

        while (await reader.ReadAsync(cancellationToken))
        {
            acessos.Add(Map(reader));
        }

        return acessos;
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            AND pessoa_id = @ColaboradorId
            ORDER BY data_hora DESC, id_acesso DESC
            LIMIT 1;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@ColaboradorId",
            colaboradorId,
            DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            return Map(reader);

        return null;
    }

    public async Task<bool> EstaNaAcademia(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM tb_acesso
            WHERE pessoa_tipo = 1
              AND pessoa_id = @ColaboradorId
              AND data_hora_saida IS NULL;
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@ColaboradorId",
            colaboradorId,
            DbType.Int32);

        var resultado = await command.ExecuteScalarAsync(
            cancellationToken);

        return Convert.ToInt32(resultado) > 0;
    }
}
