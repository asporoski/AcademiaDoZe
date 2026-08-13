// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class CepTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Cep_Obrigatorio(string? cep)
    {
        var resultado = Cep.Criar(cep!);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("12-345")]
    public void Deve_Falhar_Quando_Cep_Nao_Tiver_Oito_Digitos(string cep)
    {
        var resultado = Cep.Criar(cep);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("12345-678")]
    [InlineData("12.345-678")]
    public void Deve_Criar_Cep_Valido(string cep)
    {
        var resultado = Cep.Criar(cep);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal("12345678", resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Retornar_Valor_No_ToString()
    {
        var resultado = Cep.Criar("12345-678");

        Assert.Equal("12345678", resultado.Value!.ToString());
    }

    [Fact]
    public void Cep_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Cep.Criar("12345678");

        Assert.Empty(resultado.Notifications);
    }
}