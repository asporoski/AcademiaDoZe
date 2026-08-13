// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private static Logradouro CriarLogradouroValido()
    {
        return Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "Brasil").Value!;
    }

    private static Arquivo CriarArquivoValido()
    {
        return Arquivo.Criar(Array.Empty<byte>()).Value!;
    }

    private static Aluno CriarAlunoValido()
    {
        return Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido()).Value!;
    }

    [Fact]
    public void Deve_Criar_Acesso_Aluno_Valido()
    {
        var aluno = CriarAlunoValido();
        var dataHora = new DateTime(2026, 8, 13, 10, 0, 0);

        var resultado = AcessoAluno.Criar(
            1,
            aluno,
            dataHora);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(1, resultado.Value!.Id);
        Assert.Equal(aluno.Id, resultado.Value.AlunoId);
        Assert.Equal(dataHora, resultado.Value.DataHora);
    }

    [Fact]
    public void Deve_Falhar_Quando_Aluno_For_Nulo()
    {
        var resultado = AcessoAluno.Criar(
            1,
            null!,
            new DateTime(2026, 8, 13, 10, 0, 0));

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Deve_Falhar_Quando_Horario_For_Antes_Das_6(
        int hora)
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, hora, 0, 0));

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory]
    [InlineData(23)]
    public void Deve_Falhar_Quando_Horario_For_Depois_Das_22(
        int hora)
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, hora, 0, 0));

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(15)]
    [InlineData(18)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    public void Deve_Aceitar_Horarios_Validos(int hora)
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, hora, 0, 0));

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Deve_Aceitar_Exatamente_6_Horas()
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, 6, 0, 0));

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Deve_Aceitar_Exatamente_22_Horas()
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, 22, 0, 0));

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_As_22_Horas_E_1_Minuto()
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, 22, 1, 0));

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Fact]
    public void Acesso_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = AcessoAluno.Criar(
            1,
            CriarAlunoValido(),
            new DateTime(2026, 8, 13, 15, 30, 0));

        Assert.Empty(resultado.Notifications);
    }

    [Fact]
    public void Deve_Acumular_Erros_De_Aluno_E_Horario()
    {
        var resultado = AcessoAluno.Criar(
            1,
            null!,
            new DateTime(2026, 8, 13, 3, 0, 0));

        Assert.True(resultado.IsFailure);
        Assert.True(resultado.Notifications.Count >= 2);
    }
}