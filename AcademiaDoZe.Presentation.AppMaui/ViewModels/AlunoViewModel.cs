
// Amanda Soares Poroski

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private AlunoDto _aluno = CriarAlunoVazio();

    public AlunoDto Aluno
    {
        get => _aluno;
        set
        {
            if (SetProperty(ref _aluno, value))
            {
                AtualizarFotoPreview();
            }
        }
    }

    private ImageSource? _fotoPreview;

    public ImageSource? FotoPreview
    {
        get => _fotoPreview;
        set => SetProperty(ref _fotoPreview, value);
    }

    private int _alunoId;

    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }

    private string _confirmarSenha = string.Empty;

    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public AlunoViewModel(
        IAlunoService alunoService,
        ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;

        Title = "Cadastro de Aluno";
    }

    private static AlunoDto CriarAlunoVazio()
    {
        return new AlunoDto
        {
            Id = 0,
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento =
                DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Email = string.Empty,
            Numero = string.Empty,
            Complemento = string.Empty,
            Senha = string.Empty,
            Foto = null,
            Endereco = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            }
        };
    }

    private void AtualizarFotoPreview()
    {
        var bytes = Aluno.Foto?.Conteudo;

        if (bytes == null || bytes.Length == 0)
        {
            FotoPreview = null;
            return;
        }

        var copia = bytes.ToArray();

        FotoPreview = ImageSource.FromStream(
            () => new MemoryStream(copia));
    }

    public void NovoAluno()
    {
        AlunoId = 0;
        Aluno = CriarAlunoVazio();
        ConfirmarSenha = string.Empty;
        Title = "Cadastro de Aluno";
        AtualizarFotoPreview();
    }

    public async Task InitializeAsync()
    {
        if (AlunoId > 0)
        {
            await CarregarAlunoAsync(AlunoId);
        }
        else
        {
            NovoAluno();
        }
    }

    public async Task CarregarAlunoAsync(int id)
    {
        try
        {
            IsBusy = true;

            var aluno = await _alunoService.ObterPorIdAsync(id);

            if (aluno == null)
            {
                await Shell.Current.DisplayAlert(
                    "Aviso",
                    "Aluno não encontrado.",
                    "OK");
                return;
            }

            aluno.Endereco ??= new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            };

            aluno.Senha = string.Empty;

            Aluno = aluno;
            AlunoId = aluno.Id;
            ConfirmarSenha = string.Empty;
            Title = "Editar Aluno";

            AtualizarFotoPreview();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao carregar aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Buscar endereço pelo CEP
    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        if (IsBusy)
            return;

        if (Aluno.Endereco == null ||
            string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
        {
            await Shell.Current.DisplayAlert(
                "Aviso",
                "Informe o CEP para realizar a busca.",
                "OK");
            return;
        }

        var apenasDigitos = new string(
            Aluno.Endereco.Cep.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlert(
                "Validação",
                "O CEP deve conter exatamente 8 dígitos numéricos.",
                "OK");
            return;
        }

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradouroData =
                await _logradouroService.ObterPorCepAsync(
                    apenasDigitos, cts.Token);

            if (logradouroData != null)
            {
                Aluno.Endereco = logradouroData;

                OnPropertyChanged(nameof(Aluno));

                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    "CEP encontrado! Endereço preenchido automaticamente.",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Aviso",
                    "CEP não encontrado. Cadastre o logradouro primeiro.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "A busca por CEP demorou mais que o esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao buscar CEP: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Selecionar foto do aluno
    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha =
                await Shell.Current.DisplayActionSheet(
                    "Origem da Imagem",
                    "Cancelar",
                    "Remover Foto",
                    "Galeria",
                    "Câmera");

            if (escolha == "Remover Foto")
            {
                Aluno.Foto = null;
                FotoPreview = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }

            FileResult? result = null;

            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
            }
            else if (escolha == "Câmera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result =
                        await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Aviso",
                        "A câmera não está disponível neste dispositivo.",
                        "OK");
                    return;
                }
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

            byte[] bytes = ms.ToArray();

            if (bytes.Length == 0)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    "A imagem selecionada está vazia.",
                    "OK");
                return;
            }

            const int tamanhoMaximo = 15 * 1024 * 1024;

            if (bytes.Length > tamanhoMaximo)
            {
                await Shell.Current.DisplayAlert(
                    "Imagem muito grande",
                    "Selecione uma imagem de até 15 MB.",
                    "OK");
                return;
            }

            Aluno.Foto = new ArquivoDto
            {
                Conteudo = bytes
            };

            AtualizarFotoPreview();

            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao selecionar foto: {ex.Message}",
                "OK");
        }
    }

    // Salvar aluno
    [RelayCommand]
    private async Task SalvarAlunoAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Aluno.Nome) ||
            string.IsNullOrWhiteSpace(Aluno.Cpf) ||
            string.IsNullOrWhiteSpace(Aluno.Telefone) ||
            string.IsNullOrWhiteSpace(Aluno.Numero))
        {
            await Shell.Current.DisplayAlert(
                "Validação",
                "Preencha nome, CPF, telefone e número do endereço.",
                "OK");
            return;
        }

        if (Aluno.Endereco == null ||
            Aluno.Endereco.Id <= 0)
        {
            await Shell.Current.DisplayAlert(
                "Validação",
                "Busque um CEP cadastrado antes de salvar.",
                "OK");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Aluno.Senha) &&
            Aluno.Senha != ConfirmarSenha)
        {
            await Shell.Current.DisplayAlert(
                "Validação",
                "As senhas não coincidem.",
                "OK");
            return;
        }

        if (Aluno.Id == 0 &&
            string.IsNullOrWhiteSpace(Aluno.Senha))
        {
            await Shell.Current.DisplayAlert(
                "Validação",
                "Informe uma senha para cadastrar o aluno.",
                "OK");
            return;
        }

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Aluno.Nome = Aluno.Nome.Trim();

            Aluno.Cpf = new string(
                Aluno.Cpf.Where(char.IsDigit).ToArray());

            Aluno.Telefone = new string(
                Aluno.Telefone.Where(char.IsDigit).ToArray());

            Aluno.Numero = Aluno.Numero.Trim();

            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email)
                ? null
                : Aluno.Email.Trim();

            Aluno.Complemento =
                string.IsNullOrWhiteSpace(Aluno.Complemento)
                    ? null
                    : Aluno.Complemento.Trim();

            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha)
                ? null
                : Aluno.Senha.Trim();

            if (Aluno.Id == 0)
            {
                await _alunoService.AdicionarAsync(
                    Aluno, cts.Token);
            }
            else
            {
                await _alunoService.AtualizarAsync(
                    Aluno, cts.Token);
            }

            await Shell.Current.DisplayAlert(
                "Sucesso",
                "Aluno salvo com sucesso!",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlert(
                "Tempo Esgotado",
                "A gravação demorou mais que o esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Não foi possível salvar o aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
