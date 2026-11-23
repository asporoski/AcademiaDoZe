
// Amanda Soares Poroski

using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoColaboradorPage : ContentPage
{
    private readonly IColaboradorService _colaboradorService;
    private readonly AcessoColaboradorService _acessoService;

    private int? _colaboradorId;

    public AcessoColaboradorPage(
        IColaboradorService colaboradorService,
        AcessoColaboradorService acessoService)
    {
        InitializeComponent();

        _colaboradorService = colaboradorService
            ?? throw new ArgumentNullException(
                nameof(colaboradorService));

        _acessoService = acessoService
            ?? throw new ArgumentNullException(
                nameof(acessoService));
    }

    // ============================================
    // BUSCAR COLABORADOR PELO CPF
    // ============================================

    private async void OnBuscarClicked(
        object sender,
        EventArgs e)
    {
        _colaboradorId = null;

        EntradaButton.IsEnabled = false;
        SaidaButton.IsEnabled = false;

        NomeColaboradorLabel.Text =
            "Nenhum colaborador selecionado";

        VinculoLabel.Text =
            "Vínculo: não informado";

        SituacaoLabel.Text =
            "Buscando colaborador...";

        ResultadoLabel.Text = "";

        try
        {
            string cpf = CpfEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(cpf))
            {
                SituacaoLabel.Text =
                    "Informe o CPF do colaborador.";
                return;
            }

            var colaborador = await _colaboradorService
                .ObterPorCpfAsync(cpf);

            if (colaborador == null)
            {
                SituacaoLabel.Text =
                    "Colaborador não encontrado.";
                return;
            }

            _colaboradorId = colaborador.Id;

            NomeColaboradorLabel.Text =
                colaborador.Nome;

            VinculoLabel.Text =
                $"Vínculo: {colaborador.Vinculo}";

            await AtualizarSituacaoAsync();
        }
        catch (Exception ex)
        {
            SituacaoLabel.Text =
                "Erro ao buscar colaborador.";

            ResultadoLabel.Text = ex.Message;
        }
    }

    // ============================================
    // REGISTRAR ENTRADA
    // ============================================

    private async void OnEntradaClicked(
        object sender,
        EventArgs e)
    {
        if (!_colaboradorId.HasValue)
            return;

        int colaboradorId = _colaboradorId.Value;

        await ExecutarOperacaoAsync(
            () => _acessoService
                .RegistrarEntradaAsync(colaboradorId));
    }

    // ============================================
    // REGISTRAR SAIDA
    // ============================================

    private async void OnSaidaClicked(
        object sender,
        EventArgs e)
    {
        if (!_colaboradorId.HasValue)
            return;

        int colaboradorId = _colaboradorId.Value;

        await ExecutarOperacaoAsync(
            () => _acessoService
                .RegistrarSaidaAsync(colaboradorId));
    }

    // ============================================
    // EXECUTAR OPERACAO
    // ============================================

    private async Task ExecutarOperacaoAsync(
        Func<Task<string>> operacao)
    {
        EntradaButton.IsEnabled = false;
        SaidaButton.IsEnabled = false;
        BuscarButton.IsEnabled = false;

        try
        {
            ResultadoLabel.Text =
                "Processando...";

            string resultado = await operacao();

            ResultadoLabel.Text = resultado;
        }
        catch (Exception ex)
        {
            ResultadoLabel.Text =
                $"Erro: {ex.Message}";
        }
        finally
        {
            BuscarButton.IsEnabled = true;

            await AtualizarSituacaoAsync();
        }
    }

    // ============================================
    // ATUALIZAR SITUACAO DO COLABORADOR
    // ============================================

    private async Task AtualizarSituacaoAsync()
    {
        if (!_colaboradorId.HasValue)
        {
            EntradaButton.IsEnabled = false;
            SaidaButton.IsEnabled = false;
            return;
        }

        try
        {
            bool estaNaAcademia =
                await _acessoService
                    .EstaNaAcademiaAsync(
                        _colaboradorId.Value);

            EntradaButton.IsEnabled =
                !estaNaAcademia;

            SaidaButton.IsEnabled =
                estaNaAcademia;

            SituacaoLabel.Text = estaNaAcademia
                ? "Colaborador está na academia."
                : "Colaborador não está na academia.";
        }
        catch (Exception ex)
        {
            EntradaButton.IsEnabled = false;
            SaidaButton.IsEnabled = false;

            SituacaoLabel.Text =
                $"Erro ao verificar presença: {ex.Message}";
        }
    }
}
