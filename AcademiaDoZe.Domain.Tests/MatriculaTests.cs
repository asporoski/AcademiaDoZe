// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
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

    private static Aluno CriarAlunoValido(int idade = 20)
    {
        return Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-idade)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido()).Value!;
    }

    [Fact]
    public void Deve_Criar_Matricula_Valida()
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Ganhar massa muscular",
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(aluno.Id, resultado.Value!.AlunoId);
    }

    [Fact]
    public void Deve_Falhar_Quando_Aluno_For_Nulo()
    {
        var resultado = Matricula.Criar(
            1,
            null!,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Fact]
    public void Menor_De_16_Anos_Deve_Possuir_Laudo()
    {
        var aluno = CriarAlunoValido(15);

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "MENOR_16_LAUDO_OBRIGATORIO");
    }

    [Fact]
    public void Menor_De_16_Anos_Com_Laudo_Deve_Criar_Matricula()
    {
        var aluno = CriarAlunoValido(15);

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            CriarArquivoValido());

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_Quando_Plano_For_Invalido()
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            (MatriculaPlano)999,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "PLANO_INVALIDO");
    }

    [Fact]
    public void Deve_Falhar_Quando_DataInicio_For_Obrigatoria()
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            default,
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_INICIO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Objetivo_For_Obrigatorio(
        string? objetivo)
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            objetivo!,
            MatriculaRestricoes.None,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.PressaoAlta)]
    [InlineData(MatriculaRestricoes.Labirintite)]
    [InlineData(MatriculaRestricoes.Alergias)]
    [InlineData(MatriculaRestricoes.ProblemasRespiratorios)]
    [InlineData(MatriculaRestricoes.RemedioContinuo)]
    public void Restricao_Medica_Deve_Possuir_Laudo(
        MatriculaRestricoes restricao)
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            restricao,
            null);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "RESTRICOES_LAUDO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.PressaoAlta)]
    [InlineData(MatriculaRestricoes.Alergias)]
    public void Restricao_Com_Laudo_Deve_Criar_Matricula(
        MatriculaRestricoes restricao)
    {
        var aluno = CriarAlunoValido();

        var resultado = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            restricao,
            CriarArquivoValido());

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Plano_Mensal_Deve_Adicionar_Um_Mes()
    {
        var inicio = new DateOnly(2026, 8, 1);

        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Mensal,
            inicio,
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(
            inicio.AddMonths(1),
            resultado.Value!.DataFim);
    }

    [Fact]
    public void Plano_Trimestral_Deve_Adicionar_Tres_Meses()
    {
        var inicio = new DateOnly(2026, 8, 1);

        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Trimestral,
            inicio,
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(
            inicio.AddMonths(3),
            resultado.Value!.DataFim);
    }

    [Fact]
    public void Plano_Semestral_Deve_Adicionar_Seis_Meses()
    {
        var inicio = new DateOnly(2026, 8, 1);

        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Semestral,
            inicio,
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(
            inicio.AddMonths(6),
            resultado.Value!.DataFim);
    }

    [Fact]
    public void Plano_Anual_Deve_Adicionar_Doze_Meses()
    {
        var inicio = new DateOnly(2026, 8, 1);

        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Anual,
            inicio,
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(
            inicio.AddMonths(12),
            resultado.Value!.DataFim);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Objetivo()
    {
        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "   Ganhar   massa   muscular   ",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(
            "Ganhar massa muscular",
            resultado.Value!.Objetivo);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Das_Observacoes()
    {
        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            null,
            "   Sem   observações   importantes   ");

        Assert.Equal(
            "Sem observações importantes",
            resultado.Value!.ObservacoesRestricoes);
    }

    [Fact]
    public void Matricula_Valida_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Matricula.Criar(
            1,
            CriarAlunoValido(),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Treinar",
            MatriculaRestricoes.None,
            null);

        Assert.Empty(resultado.Notifications);
    }
}