// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class EnderecoTests
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

    [Fact]
    public void Deve_Falhar_Quando_Logradouro_For_Nulo()
    {
        var resultado = Endereco.Criar(
            null!,
            "10",
            "");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Numero_For_Obrigatorio(
        string? numero)
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            numero!,
            "");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "NUMERO_OBRIGATORIO");
    }

    [Theory]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    [InlineData("100", "Apartamento 20")]
    [InlineData("S/N", "Fundos")]
    public void Deve_Criar_Endereco_Valido(
        string numero,
        string complemento)
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            numero,
            complemento);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(logradouro.Id, resultado.Value!.LogradouroId);
        Assert.Equal(numero, resultado.Value.Numero);
        Assert.Equal(complemento, resultado.Value.Complemento);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Numero()
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            "   10   ",
            "");

        Assert.Equal(
            "10",
            resultado.Value!.Numero);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Complemento()
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            "10",
            "   Bloco A   ");

        Assert.Equal(
            "Bloco A",
            resultado.Value!.Complemento);
    }

    [Fact]
    public void Endereco_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            "10",
            "");

        Assert.Empty(resultado.Notifications);
    }

    [Fact]
    public void Deve_Armazenar_Id_Do_Logradouro()
    {
        var logradouro = CriarLogradouroValido();

        var resultado = Endereco.Criar(
            logradouro,
            "10",
            "");

        Assert.Equal(
            1,
            resultado.Value!.LogradouroId);
    }
}