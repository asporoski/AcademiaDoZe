
// Amanda Soares Poroski

using AcademiaDoZe.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// Classe auxiliar para seleção múltipla
// de restrições médicas usando enum Flags.
public class RestricaoSelectable : ObservableObject
{
    public AppMatriculaRestricoes Value { get; set; }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
