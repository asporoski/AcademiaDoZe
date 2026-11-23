
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository
    : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(
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
        WHERE pessoa_tipo = 0
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

    private static AcessoAluno Map(
        DbDataReader reader)
    {
        int id = Convert.ToInt32(
            reader.GetValue(0));

        int alunoId = Convert.ToInt32(
            reader.GetValue(1));

        DateTime entrada = LerData(reader, 2);

        DateTime? saida = reader.IsDBNull(3)
            ? null
            : LerData(reader, 3);

        return AcessoAluno.Reconstituir(
            id,
            alunoId,
            entrada,
            saida);
    }

    // ============================================
    // OBTER POR ID
    // ============================================

    public async Task<AcessoAluno?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            AND id_acesso = @Id;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@Id", id, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            return Map(reader);

        return null;
    }

    // ============================================
    // OBTER TODOS
    // ============================================

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            ORDER BY data_hora DESC, id_acesso DESC;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var acessos = new List<AcessoAluno>();

        while (await reader.ReadAsync(cancellationToken))
        {
            acessos.Add(Map(reader));
        }

        return acessos;
    }

    // ============================================
    // ADICIONAR ENTRADA
    // ============================================

    public async Task<AcessoAluno> Adicionar(
        AcessoAluno entity,
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
                0,
                @AlunoId,
                @Entrada,
                @Saida
            );

            SELECT last_insert_rowid();
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@AlunoId",
            entity.AlunoId,
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

        return AcessoAluno.Reconstituir(
            id,
            entity.AlunoId,
            entity.DataHoraEntrada,
            entity.DataHoraSaida);
    }

    // ============================================
    // ATUALIZAR ACESSO / SALVAR SAIDA
    // ============================================

    public async Task<AcessoAluno> Atualizar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        const string sql = """
            UPDATE tb_acesso
            SET
                data_hora = @Entrada,
                data_hora_saida = @Saida
            WHERE id_acesso = @Id
              AND pessoa_tipo = 0
              AND pessoa_id = @AlunoId;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@Id",
            entity.Id,
            DbType.Int32);

        command.AddParameter(
            "@AlunoId",
            entity.AlunoId,
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
                "Acesso do aluno não encontrado.");
        }

        return entity;
    }

    // ============================================
    // REMOVER ACESSO
    // ============================================

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM tb_acesso
            WHERE id_acesso = @Id
              AND pessoa_tipo = 0;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@Id",
            id,
            DbType.Int32);

        int linhas = await command.ExecuteNonQueryAsync(
            cancellationToken);

        return linhas > 0;
    }

    // ============================================
    // ACESSOS POR ALUNO E PERIODO
    // ============================================

    public async Task<IEnumerable<AcessoAluno>>
        ObterAcessosPorAlunoPeriodo(
            int? alunoId = null,
            DateOnly? inicio = null,
            DateOnly? fim = null,
            CancellationToken cancellationToken = default)
    {
        if (inicio.HasValue && fim.HasValue &&
            inicio.Value > fim.Value)
        {
            throw new ArgumentException(
                "A data inicial não pode superar a final.");
        }

        string sql = $"""
            {SelectBase}
            AND (@AlunoId IS NULL OR pessoa_id = @AlunoId)
            AND (@Inicio IS NULL OR data_hora >= @Inicio)
            AND (@Fim IS NULL OR data_hora < @Fim)
            ORDER BY data_hora DESC, id_acesso DESC;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@AlunoId",
            alunoId.HasValue
                ? alunoId.Value
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
            await command.ExecuteReaderAsync(
                cancellationToken);

        var acessos = new List<AcessoAluno>();

        while (await reader.ReadAsync(cancellationToken))
        {
            acessos.Add(Map(reader));
        }

        return acessos;
    }

    // ============================================
    // ULTIMO ACESSO DO ALUNO
    // ============================================

    public async Task<AcessoAluno?> ObterUltimoAcesso(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {SelectBase}
            AND pessoa_id = @AlunoId
            ORDER BY data_hora DESC, id_acesso DESC
            LIMIT 1;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@AlunoId",
            alunoId,
            DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            return Map(reader);

        return null;
    }

    // ============================================
    // VERIFICAR SE ESTA NA ACADEMIA
    // ============================================

    public async Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM tb_acesso
            WHERE pessoa_tipo = 0
              AND pessoa_id = @AlunoId
              AND data_hora_saida IS NULL;
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        command.AddParameter(
            "@AlunoId",
            alunoId,
            DbType.Int32);

        var resultado = await command.ExecuteScalarAsync(
            cancellationToken);

        return Convert.ToInt32(resultado) > 0;
    }

    // ============================================
    // ALUNOS SEM ACESSO NOS ULTIMOS DIAS
    // ============================================

    public async Task<IEnumerable<Aluno>>
        ObterAlunosSemAcessoNosUltimosDias(
            int dias,
            CancellationToken cancellationToken = default)
    {
        if (dias < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dias),
                "A quantidade de dias não pode ser negativa.");
        }

        const string sql = """
            SELECT a.id_aluno
            FROM tb_aluno a
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM tb_acesso ac
                WHERE ac.pessoa_tipo = 0
                  AND ac.pessoa_id = a.id_aluno
                  AND ac.data_hora >= @DataLimite
            )
            ORDER BY a.nome;
            """;

        var ids = new List<int>();

        await using (var command =
            await CreateCommandAsync(
                sql,
                cancellationToken))
        {
            command.AddParameter(
                "@DataLimite",
                FormatarData(DateTime.Now.AddDays(-dias)),
                DbType.String);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                ids.Add(Convert.ToInt32(reader.GetValue(0)));
            }
        }

        var alunos = new List<Aluno>();

        await using var alunoRepository =
            new AlunoRepository(
                _connectionString,
                _databaseType);

        foreach (int id in ids)
        {
            var aluno = await alunoRepository.ObterPorId(
                id,
                cancellationToken);

            if (aluno != null)
                alunos.Add(aluno);
        }

        return alunos;
    }
}
