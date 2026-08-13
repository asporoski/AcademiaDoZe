// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoAlunoRepository : IRepository<AcessoAluno>
{
    Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default);

    Task<AcessoAluno?> ObterUltimoAcesso(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(
        int dias,
        CancellationToken cancellationToken = default);
}