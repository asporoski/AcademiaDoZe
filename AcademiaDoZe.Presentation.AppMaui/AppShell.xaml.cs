
// Amanda Soares Poroski

using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        // Rota de Logradouros
        Routing.RegisterRoute(
            "logradouro",
            typeof(LogradouroPage));

        // Rota de cadastro e edição de Colaboradores
        Routing.RegisterRoute(
            "colaborador",
            typeof(ColaboradorPage));

        // Rota de cadastro e edição de Alunos
        Routing.RegisterRoute(
            "aluno",
            typeof(AlunoPage));

        // Rota de cadastro e edição de Matrículas
        Routing.RegisterRoute(
            "matricula",
            typeof(MatriculaPage));
    }
}
