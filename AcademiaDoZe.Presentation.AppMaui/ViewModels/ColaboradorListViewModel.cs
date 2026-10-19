
// Amanda Soares Poroski

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// Item usado para mostrar a foto na listagem.
public class ColaboradorListaItem
{
    public ColaboradorDto Colaborador { get; set; } = null!;

    public int Id => Colaborador.Id;
    public string Nome => Colaborador.Nome;
    public string Cpf => Colaborador.Cpf;
    public string? Email => Colaborador.Email;

    public ImageSource? FotoPreview
    {
        get
        {
            var bytes = Colaborador.Foto?.Conteudo;

            if (bytes == null || bytes.Length == 0)
                return null;

            var copia = bytes.ToArray();

            return ImageSource.FromStream(
                () => new MemoryStream(copia));
        }
    }
}

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    public ObservableCollection<string> FilterTypes { get; } =
        ["Nome", "CPF", "Id", "Email", "Tipo", "Vínculo"];

    private ObservableCollection<ColaboradorDto> _colaboradores = [];

    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set
        {
            if (SetProperty(ref _colaboradores, value))
                AtualizarItensDaLista();
        }
    }

    // Coleção para a interface mostrar as fotos.
    public ObservableCollection<ColaboradorListaItem> ColaboradoresComFoto
    { get; } = [];

    private ColaboradorDto? _selectedColaborador;

    public ColaboradorDto? SelectedColaborador
    {
        get => _selectedColaborador;
        set => SetProperty(ref _selectedColaborador, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(
        IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    private void AtualizarItensDaLista()
    {
        ColaboradoresComFoto.Clear();

        foreach (var colaborador in Colaboradores)
        {
            ColaboradoresComFoto.Add(
                new ColaboradorListaItem
                {
                    Colaborador = colaborador
                });
        }
    }

    // Abrir cadastro
    [RelayCommand]
    private async Task AddColaboradorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("colaborador");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao navegar para tela de cadastro: {ex.Message}",
                "OK");
        }
    }

    // Editar colaborador
    [RelayCommand]
    private async Task EditColaboradorAsync(
        ColaboradorDto colaborador)
    {
        try
        {
            if (colaborador == null)
                return;

            await Shell.Current.GoToAsync(
                $"colaborador?Id={colaborador.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao navegar para tela de edição: {ex.Message}",
                "OK");
        }
    }

    // Atualizar lista
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    // Pesquisar colaboradores
    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            IEnumerable<ColaboradorDto> resultados = [];

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados =
                    await _colaboradorService.ObterTodosAsync(cts.Token)
                    ?? [];
            }
            else if (SelectedFilterType == "Nome")
            {
                var todos =
                    await _colaboradorService.ObterTodosAsync(cts.Token)
                    ?? [];

                var termo = SearchText.Trim();

                resultados = todos.Where(c =>
                    c.Nome.Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpfLimpo = new string(
                    SearchText.Where(char.IsDigit).ToArray());

                if (cpfLimpo.Length != 11)
                {
                    await Shell.Current.DisplayAlert(
                        "Validação",
                        "Para buscar por CPF, informe os 11 dígitos numéricos.",
                        "OK");
                    return;
                }

                var colaborador =
                    await _colaboradorService.ObterPorCpfAsync(
                        cpfLimpo, cts.Token);

                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id)
                    || id <= 0)
                {
                    await Shell.Current.DisplayAlert(
                        "Validação",
                        "Para buscar por ID, informe um número inteiro positivo válido.",
                        "OK");
                    return;
                }

                var colaborador =
                    await _colaboradorService.ObterPorIdAsync(
                        id, cts.Token);

                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Email")
            {
                var colaborador =
                    await _colaboradorService.ObterPorEmailAsync(
                        SearchText.Trim(), cts.Token);

                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Tipo")
            {
                var termo = SearchText.Trim();

                var correspondentes =
                    Enum.GetValues<AppColaboradorTipo>()
                    .Where(t =>
                        t.ToString().Contains(
                            termo,
                            StringComparison.OrdinalIgnoreCase)
                        || t.GetDisplayName().Contains(
                            termo,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (correspondentes.Count == 0)
                {
                    await Shell.Current.DisplayAlert(
                        "Validação",
                        "Informe um tipo válido para filtrar.",
                        "OK");
                    return;
                }

                var listaPorTipo = new List<ColaboradorDto>();

                foreach (var tipo in correspondentes)
                {
                    var itens =
                        await _colaboradorService.ObterPorTipoAsync(
                            tipo, cts.Token);

                    if (itens != null)
                        listaPorTipo.AddRange(itens);
                }

                resultados =
                    listaPorTipo.DistinctBy(c => c.Id);
            }
            else if (SelectedFilterType == "Vínculo")
            {
                var termo = SearchText.Trim();

                var correspondentes =
                    Enum.GetValues<AppColaboradorVinculo>()
                    .Where(v =>
                        v.ToString().Contains(
                            termo,
                            StringComparison.OrdinalIgnoreCase)
                        || v.GetDisplayName().Contains(
                            termo,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (correspondentes.Count == 0)
                {
                    await Shell.Current.DisplayAlert(
                        "Validação",
                        "Informe um vínculo válido para filtrar.",
                        "OK");
                    return;
                }

                var listaPorVinculo = new List<ColaboradorDto>();

                foreach (var vinculo in correspondentes)
                {
                    var itens =
                        await _colaboradorService.ObterPorVinculoAsync(
                            vinculo, cts.Token);

                    if (itens != null)
                        listaPorVinculo.AddRange(itens);
                }

                resultados =
                    listaPorVinculo.DistinctBy(c => c.Id);
            }

            var lista = resultados.ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();

                foreach (var item in lista)
                    Colaboradores.Add(item);

                AtualizarItensDaLista();

                OnPropertyChanged(nameof(Colaboradores));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "A busca de colaboradores expirou. Verifique a conexão com o banco.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao buscar colaboradores: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Carregar colaboradores do banco de dados
    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var colaboradoresList =
                await _colaboradorService.ObterTodosAsync(cts.Token);

            var lista = colaboradoresList?.ToList()
                ?? new List<ColaboradorDto>();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();

                foreach (var colaborador in lista)
                    Colaboradores.Add(colaborador);

                AtualizarItensDaLista();

                OnPropertyChanged(nameof(Colaboradores));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "O carregamento dos colaboradores expirou. Verifique a conexão.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao carregar colaboradores: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    // Excluir colaborador
    [RelayCommand]
    private async Task DeleteColaboradorAsync(
        ColaboradorDto colaborador)
    {
        if (colaborador == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Sim",
            "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            bool success =
                await _colaboradorService.RemoverAsync(
                    colaborador.Id, cts.Token);

            if (success)
            {
                Colaboradores.Remove(colaborador);

                await MainThread.InvokeOnMainThreadAsync(
                    AtualizarItensDaLista);

                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    "Colaborador excluído com sucesso!",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    "Não foi possível excluir o colaborador.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "A exclusão do colaborador expirou. Verifique a conexão.",
                "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains(
                    "FOREIGN KEY",
                    StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains(
                    "constraint",
                    StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains(
                    "REFERENCE",
                    StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlert(
                    "Não Permitido",
                    "Este colaborador não pode ser excluído pois possui registros vinculados no sistema (ex: acessos, turmas ou matrículas).",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Erro ao excluir colaborador: {ex.Message}",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
