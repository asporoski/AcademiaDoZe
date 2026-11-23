
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository>
        _acessoRepoFactory;

    private readonly Func<IColaboradorRepository>
        _colaboradorRepoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> acessoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory
            ?? throw new ArgumentNullException(
                nameof(acessoRepoFactory));

        _colaboradorRepoFactory = colaboradorRepoFactory
            ?? throw new ArgumentNullException(
                nameof(colaboradorRepoFactory));
    }

    // ============================================
    // LIMITE DIARIO CONFORME O VINCULO
    // ============================================

    private static TimeSpan ObterLimiteDiario(
        ColaboradorVinculo vinculo)
    {
        return vinculo switch
        {
            ColaboradorVinculo.CLT =>
                TimeSpan.FromHours(8),

            ColaboradorVinculo.Estagio =>
                TimeSpan.FromHours(6),

            _ => throw new InvalidOperationException(
                "Vínculo do colaborador inválido.")
        };
    }

    // ============================================
    // TOTAL DE HORAS TRABALHADAS NO DIA
    // ============================================

    private async Task<TimeSpan> ObterTotalDiaAsync(
        int colaboradorId,
        DateOnly data,
        CancellationToken cancellationToken)
    {
        var acessos = await _acessoRepoFactory()
            .ObterAcessosPorColaboradorPeriodo(
                colaboradorId,
                data,
                data,
                cancellationToken);

        TimeSpan total = TimeSpan.Zero;

        foreach (var acesso in acessos)
        {
            var fim = acesso.DataHoraSaida
                ?? DateTime.Now;

            if (fim < acesso.DataHoraEntrada)
                continue;

            total += fim - acesso.DataHoraEntrada;
        }

        return total;
    }

    // ============================================
    // REGISTRAR ENTRADA
    // ============================================

    public async Task<string> RegistrarEntradaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepoFactory()
            .ObterPorId(
                colaboradorId,
                cancellationToken);

        if (colaborador == null)
            return "Colaborador não encontrado.";

        bool estaNaAcademia = await _acessoRepoFactory()
            .EstaNaAcademia(
                colaboradorId,
                cancellationToken);

        if (estaNaAcademia)
        {
            return "Entrada não permitida! O colaborador já possui uma entrada em aberto.";
        }

        var hoje = DateOnly.FromDateTime(DateTime.Now);

        var totalDia = await ObterTotalDiaAsync(
            colaboradorId,
            hoje,
            cancellationToken);

        var limite = ObterLimiteDiario(
            colaborador.Vinculo);

        if (totalDia >= limite)
        {
            return
                $"Entrada negada!\n" +
                $"Colaborador: {colaborador.Nome}\n" +
                $"Limite diário de {limite.TotalHours:0} horas atingido.";
        }

        var resultado = AcessoColaborador.Criar(
            0,
            colaborador,
            DateTime.Now);

        if (resultado.IsFailure)
        {
            return "Não foi possível registrar a entrada. Verifique o horário permitido.";
        }

        await _acessoRepoFactory()
            .Adicionar(
                resultado.Value!,
                cancellationToken);

        var restante = limite - totalDia;

        return
            $"Entrada autorizada!\n" +
            $"Colaborador: {colaborador.Nome}\n" +
            $"Vínculo: {colaborador.Vinculo}\n" +
            $"Tempo disponível hoje: " +
            $"{(int)restante.TotalHours}h {restante.Minutes}min";
    }

    // ============================================
    // REGISTRAR SAIDA
    // ============================================

    public async Task<string> RegistrarSaidaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepoFactory()
            .ObterPorId(
                colaboradorId,
                cancellationToken);

        if (colaborador == null)
            return "Colaborador não encontrado.";

        var acesso = await _acessoRepoFactory()
            .ObterUltimoAcesso(
                colaboradorId,
                cancellationToken);

        if (acesso == null || !acesso.EstaNaAcademia)
        {
            return "Saída não permitida! O colaborador não possui entrada em aberto.";
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

        var hoje = DateOnly.FromDateTime(DateTime.Now);

        var totalDia = await ObterTotalDiaAsync(
            colaboradorId,
            hoje,
            cancellationToken);

        var limite = ObterLimiteDiario(
            colaborador.Vinculo);

        return
            $"Saída registrada!\n" +
            $"Colaborador: {colaborador.Nome}\n" +
            $"Tempo desta permanência: " +
            $"{(int)tempo.TotalHours}h {tempo.Minutes}min\n" +
            $"Total trabalhado hoje: " +
            $"{(int)totalDia.TotalHours}h {totalDia.Minutes}min\n" +
            $"Limite diário: {limite.TotalHours:0} horas";
    }

    // ============================================
    // VERIFICAR PRESENCA
    // ============================================

    public async Task<bool> EstaNaAcademiaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory()
            .EstaNaAcademia(
                colaboradorId,
                cancellationToken);
    }

    // ============================================
    // CONSULTAR HISTORICO
    // ============================================

    public async Task<IEnumerable<AcessoColaborador>>
        ObterHistoricoAsync(
            int colaboradorId,
            CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory()
            .ObterAcessosPorColaboradorPeriodo(
                colaboradorId,
                cancellationToken: cancellationToken);
    }
}
