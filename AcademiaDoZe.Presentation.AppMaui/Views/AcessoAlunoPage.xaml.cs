
// Amanda Soares Poroski

using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoAlunoPage : ContentPage
{
    private readonly IAlunoService _alunoService;
    private readonly AcessoAlunoService _acessoService;

    private int? _alunoId;

    public AcessoAlunoPage(
        IAlunoService alunoService,
        AcessoAlunoService acessoService)
    {
        InitializeComponent();

        _alunoService = alunoService
            ?? throw new ArgumentNullException(
                nameof(alunoService));

        _acessoService = acessoService
            ?? throw new ArgumentNullException(
                nameof(acessoService));
    }

    // ============================================
    // BUSCAR ALUNO PELO CPF
    // ============================================

    private async void OnBuscarClicked(
        object sender,
        EventArgs e)
    {
        _alunoId = null;

        EntradaButton.IsEnabled = false;
        SaidaButton.IsEnabled = false;

        NomeAlunoLabel.Text =
            "Nenhum aluno selecionado";

        SituacaoLabel.Text =
            "Buscando aluno...";

        ResultadoLabel.Text = "";

        try
        {
            string cpf = CpfEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(cpf))
            {
                SituacaoLabel.Text =
                    "Informe o CPF do aluno.";
                return;
            }

            var aluno = await _alunoService
                .ObterPorCpfAsync(cpf);

            if (aluno == null)
            {
                SituacaoLabel.Text =
                    "Aluno não encontrado.";
                return;
            }

            _alunoId = aluno.Id;

            NomeAlunoLabel.Text = aluno.Nome;

            await AtualizarSituacaoAsync();
        }
        catch (Exception ex)
        {
            SituacaoLabel.Text =
                "Erro ao buscar aluno.";

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
        if (!_alunoId.HasValue)
            return;

        int alunoId = _alunoId.Value;

        await ExecutarOperacaoAsync(
            () => _acessoService
                .RegistrarEntradaAsync(alunoId));
    }

    // ============================================
    // REGISTRAR SAIDA
    // ============================================

    private async void OnSaidaClicked(
        object sender,
        EventArgs e)
    {
        if (!_alunoId.HasValue)
            return;

        int alunoId = _alunoId.Value;

        await ExecutarOperacaoAsync(
            () => _acessoService
                .RegistrarSaidaAsync(alunoId));
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
    // ATUALIZAR SITUACAO DO ALUNO
    // ============================================

    private async Task AtualizarSituacaoAsync()
    {
        if (!_alunoId.HasValue)
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
                        _alunoId.Value);

            EntradaButton.IsEnabled =
                !estaNaAcademia;

            SaidaButton.IsEnabled =
                estaNaAcademia;

            SituacaoLabel.Text = estaNaAcademia
                ? "Aluno está na academia."
                : "Aluno não está na academia.";
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
