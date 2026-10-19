
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(
        ColaboradorListViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
        }
    }
}
