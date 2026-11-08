
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    private bool _suspendSelectionSync;

    public IEnumerable<AppMatriculaPlano> PlanoTipos { get; } =
        Enum.GetValues<AppMatriculaPlano>();

    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = new();

    private MatriculaDto _matricula = CriarMatriculaVazia();

    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            if (SetProperty(ref _matricula, value))
            {
                SyncSelectionFromFlags();
                AtualizarLaudoPreview();
                NotificarPropriedadesVisuais();
            }
        }
    }

    private int _matriculaId;

    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private bool _isEditMode;

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private ImageSource? _laudoPreview;

    public ImageSource? LaudoPreview
    {
        get => _laudoPreview;
        set => SetProperty(ref _laudoPreview, value);
    }

    public bool HasAlunoVinculado =>
        Matricula.AlunoMatricula?.Id > 0;

    public bool HasNoAlunoVinculado =>
        !HasAlunoVinculado;

    public bool IsMenor16 =>
        Matricula.AlunoMatricula != null &&
        Matricula.AlunoMatricula.DataNascimento != default &&
        Matricula.AlunoMatricula.DataNascimento >
        DateOnly.FromDateTime(DateTime.Today.AddYears(-16));

    public bool HasRestricoes =>
        RestricoesMulti.Any(x => x.IsSelected);

    public bool ExigeLaudo =>
        IsMenor16 || HasRestricoes;

    public bool HasLaudo =>
        Matricula.LaudoMedico?.Conteudo?.Length > 0;

    public bool LaudoPendente =>
        ExigeLaudo && !HasLaudo;

    public bool LaudoOpcional =>
        !ExigeLaudo && !HasLaudo;

    public MatriculaViewModel(
        IMatriculaService matriculaService,
        IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;

        Title = "Nova Matrícula";

        BuildRestricoesMulti();
    }

    private static MatriculaDto CriarMatriculaVazia()
    {
        return new MatriculaDto
        {
            AlunoMatricula = new AlunoDto
            {
                Nome = string.Empty,
                Cpf = string.Empty,
                DataNascimento = DateOnly.FromDateTime(
                    DateTime.Today.AddYears(-18)),
                Telefone = string.Empty,
                Numero = string.Empty,
                Endereco = new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                }
            },
            Plano = AppMatriculaPlano.Anual,
            DataInicio = DateOnly.FromDateTime(DateTime.Today),
            DataFim = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Objetivo = string.Empty,
            RestricoesMedicas = AppMatriculaRestricoes.None,
            ObservacoesRestricoes = string.Empty,
            LaudoMedico = null
        };
    }

    public void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }

    public async Task InitializeAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matrícula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matrícula";
            Matricula = CriarMatriculaVazia();
        }

        NotificarPropriedadesVisuais();
    }

    private void BuildRestricoesMulti()
    {
        foreach (var item in RestricoesMulti)
        {
            item.PropertyChanged -= RestricaoItem_PropertyChanged;
        }

        RestricoesMulti.Clear();

        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None)
                continue;

            var numero = Convert.ToInt64(value);

            if (numero <= 0 || (numero & (numero - 1)) != 0)
                continue;

            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };

            item.PropertyChanged += RestricaoItem_PropertyChanged;

            RestricoesMulti.Add(item);
        }

        NotificarPropriedadesVisuais();
    }

    private void RestricaoItem_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (_suspendSelectionSync)
            return;

        if (e.PropertyName == nameof(RestricaoSelectable.IsSelected))
        {
            RecomputeRestricoesFlags();
        }
    }

    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.None;

        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected)
            {
                flags |= item.Value;
            }
        }

        Matricula.RestricoesMedicas = flags;

        NotificarPropriedadesVisuais();
    }

    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;

        try
        {
            foreach (var item in RestricoesMulti)
            {
                item.IsSelected =
                    Matricula.RestricoesMedicas.HasFlag(item.Value);
            }
        }
        finally
        {
            _suspendSelectionSync = false;
        }

        NotificarPropriedadesVisuais();
    }

    private void AtualizarLaudoPreview()
    {
        var bytes = Matricula.LaudoMedico?.Conteudo;

        if (bytes == null || bytes.Length == 0)
        {
            LaudoPreview = null;
        }
        else
        {
            var copia = bytes.ToArray();

            LaudoPreview = ImageSource.FromStream(
                () => new MemoryStream(copia));
        }

        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;

        AtualizarLaudoPreview();
        OnPropertyChanged(nameof(Matricula));
    }

    [RelayCommand]
    public async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var dados = await _matriculaService.ObterPorIdAsync(
                MatriculaId,
                cts.Token);

            if (dados != null)
            {
                Matricula = dados;
                SyncSelectionFromFlags();
                AtualizarLaudoPreview();
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Aviso",
                    "Matrícula não encontrada.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao carregar matrícula: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(
            Matricula.AlunoMatricula?.Cpf))
        {
            await Shell.Current.DisplayAlert(
                "Aviso",
                "Informe o CPF do aluno.",
                "OK");
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var cpf = new string(
                Matricula.AlunoMatricula.Cpf
                    .Where(char.IsDigit)
                    .ToArray());

            var aluno = await _alunoService.ObterPorCpfAsync(
                cpf,
                cts.Token);

            if (aluno != null)
            {
                Matricula.AlunoMatricula = aluno;

                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();

                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    "Aluno encontrado!",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Aviso",
                    "Aluno não encontrado.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao buscar CPF: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheet(
                "Origem do Laudo Médico",
                "Cancelar",
                null,
                "Galeria",
                "Câmera",
                "Remover Laudo");

            if (escolha == "Remover Laudo")
            {
                RemoverLaudo();
                return;
            }

            FileResult? result = null;

            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Selecione o laudo médico",
                        FileTypes = FilePickerFileType.Images
                    });
            }
            else if (escolha == "Câmera")
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert(
                        "Aviso",
                        "Câmera indisponível.",
                        "OK");
                    return;
                }

                result = await MediaPicker.Default.CapturePhotoAsync();
            }
            else
            {
                return;
            }

            if (result == null)
                return;

            using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();

            await stream.CopyToAsync(ms);

            var bytes = ms.ToArray();

            if (bytes.Length == 0)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    "O arquivo está vazio.",
                    "OK");
                return;
            }

            const int tamanhoMaximo = 15 * 1024 * 1024;

            if (bytes.Length > tamanhoMaximo)
            {
                await Shell.Current.DisplayAlert(
                    "Arquivo muito grande",
                    "Selecione uma imagem de até 15 MB.",
                    "OK");
                return;
            }

            Matricula.LaudoMedico = new ArquivoDto
            {
                Conteudo = bytes
            };

            AtualizarLaudoPreview();
            OnPropertyChanged(nameof(Matricula));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao selecionar laudo: {ex.Message}",
                "OK");
        }
    }

    private static async Task<bool> ValidateMatriculaAsync(
        MatriculaDto matricula)
    {
        var erros = new List<string>();

        if (matricula.AlunoMatricula == null ||
            matricula.AlunoMatricula.Id <= 0)
        {
            erros.Add("• Selecione um aluno válido pelo CPF.");
        }

        if (matricula.DataInicio == default)
        {
            erros.Add("• Informe a data de início.");
        }

        if (string.IsNullOrWhiteSpace(matricula.Objetivo))
        {
            erros.Add("• Informe o objetivo da matrícula.");
        }

        if (!Enum.IsDefined(matricula.Plano))
        {
            erros.Add("• Selecione um plano válido.");
        }

        bool menor16 =
            matricula.AlunoMatricula != null &&
            matricula.AlunoMatricula.DataNascimento != default &&
            matricula.AlunoMatricula.DataNascimento >
            DateOnly.FromDateTime(DateTime.Today.AddYears(-16));

        bool possuiLaudo =
            matricula.LaudoMedico?.Conteudo?.Length > 0;

        if (menor16 && !possuiLaudo)
        {
            erros.Add(
                "• Alunos menores de 16 anos precisam de laudo médico.");
        }

        if (matricula.RestricoesMedicas !=
            AppMatriculaRestricoes.None && !possuiLaudo)
        {
            erros.Add(
                "• Alunos com restrições médicas precisam de laudo médico.");
        }

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlert(
                "Erros de Validação",
                "Corrija os seguintes campos:\n\n" +
                string.Join("\n", erros),
                "OK");

            return false;
        }

        return true;
    }

    [RelayCommand]
    public async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;

        RecomputeRestricoesFlags();

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            AlunoDto? aluno = null;

            if (Matricula.AlunoMatricula?.Id > 0)
            {
                aluno = await _alunoService.ObterPorIdAsync(
                    Matricula.AlunoMatricula.Id,
                    cts.Token);
            }
            else if (!string.IsNullOrWhiteSpace(
                Matricula.AlunoMatricula?.Cpf))
            {
                var cpf = new string(
                    Matricula.AlunoMatricula.Cpf
                        .Where(char.IsDigit)
                        .ToArray());

                aluno = await _alunoService.ObterPorCpfAsync(
                    cpf,
                    cts.Token);
            }

            if (aluno == null)
            {
                await Shell.Current.DisplayAlert(
                    "Validação",
                    "Busque um aluno válido pelo CPF.",
                    "OK");
                return;
            }

            Matricula.AlunoMatricula = aluno;

            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();

            if (!await ValidateMatriculaAsync(Matricula))
                return;

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(
                    Matricula,
                    cts.Token);
            }
            else
            {
                await _matriculaService.AdicionarAsync(
                    Matricula,
                    cts.Token);
            }

            await Shell.Current.DisplayAlert(
                "Sucesso",
                IsEditMode
                    ? "Matrícula atualizada com sucesso!"
                    : "Matrícula cadastrada com sucesso!",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "A operação demorou mais que o esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao salvar matrícula: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
