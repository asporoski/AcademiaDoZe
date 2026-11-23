
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository>
        _acessoRepoFactory;

    private readonly Func<IAlunoRepository>
        _alunoRepoFactory;

    private readonly Func<IMatriculaRepository>
        _matriculaRepoFactory;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> acessoRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory
            ?? throw new ArgumentNullException(
                nameof(acessoRepoFactory));

        _alunoRepoFactory = alunoRepoFactory
            ?? throw new ArgumentNullException(
                nameof(alunoRepoFactory));

        _matriculaRepoFactory = matriculaRepoFactory
            ?? throw new ArgumentNullException(
                nameof(matriculaRepoFactory));
    }

    // ============================================
    // REGISTRAR ENTRADA DO ALUNO
    // ============================================

    public async Task<string> RegistrarEntradaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepoFactory()
            .ObterPorId(alunoId, cancellationToken);

        if (aluno == null)
            return "Aluno não encontrado.";

        var matricula = await _matriculaRepoFactory()
            .ObterMatriculaAtivaPorAluno(
                alunoId,
                cancellationToken);

        if (matricula == null || !matricula.EstaAtiva)
        {
            return "Acesso negado! O aluno não possui matrícula ativa.";
        }

        bool estaNaAcademia = await _acessoRepoFactory()
            .EstaNaAcademia(alunoId, cancellationToken);

        if (estaNaAcademia)
        {
            return "Entrada não permitida! O aluno já está na academia.";
        }

        var resultado = AcessoAluno.Criar(
            0,
            aluno,
            DateTime.Now);

        if (resultado.IsFailure)
        {
            return "Não foi possível registrar a entrada. Verifique o horário permitido.";
        }

        await _acessoRepoFactory()
            .Adicionar(
                resultado.Value!,
                cancellationToken);

        return
            $"Entrada autorizada!\n" +
            $"Aluno: {aluno.Nome}\n" +
            $"Dias restantes do plano: {matricula.DiasRestantes}";
    }

    // ============================================
    // REGISTRAR SAIDA DO ALUNO
    // ============================================

    public async Task<string> RegistrarSaidaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepoFactory()
            .ObterPorId(alunoId, cancellationToken);

        if (aluno == null)
            return "Aluno não encontrado.";

        var acesso = await _acessoRepoFactory()
            .ObterUltimoAcesso(
                alunoId,
                cancellationToken);

        if (acesso == null || !acesso.EstaNaAcademia)
        {
            return "Saída não permitida! O aluno não possui entrada em aberto.";
        }

        var resultado = acesso.RegistrarSaida(
            DateTime.Now);

        if (resultado.IsFailure)
        {
            return "Não foi possível registrar a saída. Verifique o horário.";
        }

        await _acessoRepoFactory()
            .Atualizar(
                acesso,
                cancellationToken);

        var tempo = acesso.TempoPermanencia
            ?? TimeSpan.Zero;

        return
            $"Saída registrada!\n" +
            $"Aluno: {aluno.Nome}\n" +
            $"Tempo de permanência: " +
            $"{(int)tempo.TotalHours}h {tempo.Minutes}min";
    }

    // ============================================
    // VERIFICAR PRESENCA
    // ============================================

    public async Task<bool> EstaNaAcademiaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory()
            .EstaNaAcademia(
                alunoId,
                cancellationToken);
    }

    // ============================================
    // CONSULTAR HISTORICO
    // ============================================

    public async Task<IEnumerable<AcessoAluno>>
        ObterHistoricoAsync(
            int alunoId,
            CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory()
            .ObterAcessosPorAlunoPeriodo(
                alunoId,
                cancellationToken: cancellationToken);
    }
}
