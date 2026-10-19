
// Amanda Soares Poroski

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class AlunoListaItem
{
    public AlunoDto Aluno { get; set; } = null!;

    public int Id => Aluno.Id;

    public string Nome => Aluno.Nome;

    public string Cpf => Aluno.Cpf;

    public string? Email => Aluno.Email;

    public ImageSource? FotoPreview
    {
        get
        {
            var bytes = Aluno.Foto?.Conteudo;

            if (bytes == null || bytes.Length == 0)
                return null;

            return ImageSource.FromStream(
                () => new MemoryStream(bytes));
        }
    }
}

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<AlunoListaItem> Alunos { get; } = [];

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var alunos = await _alunoService.ObterTodosAsync();

            Alunos.Clear();

            if (alunos != null)
            {
                foreach (var aluno in alunos)
                {
                    Alunos.Add(new AlunoListaItem
                    {
                        Aluno = aluno
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao carregar alunos: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        await Shell.Current.GoToAsync("aluno");
    }

    [RelayCommand]
    private async Task EditAlunoAsync(AlunoListaItem item)
    {
        if (item == null)
            return;

        await Shell.Current.GoToAsync(
            $"aluno?Id={item.Id}");
    }
}
