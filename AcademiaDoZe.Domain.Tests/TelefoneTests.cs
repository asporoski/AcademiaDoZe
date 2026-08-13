// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class TelefoneTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Telefone_Obrigatorio(string? telefone)
    {
        var resultado = Telefone.Criar(telefone!);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("47-9999")]
    public void Deve_Falhar_Quando_Telefone_Nao_Tiver_Onze_Digitos(
        string telefone)
    {
        var resultado = Telefone.Criar(telefone);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory]
    [InlineData("47999999999")]
    [InlineData("(47)99999-9999")]
    [InlineData("47 99999-9999")]
    [InlineData("(49) 98888-7777")]
    public void Deve_Criar_Telefone_Valido(string telefone)
    {
        var resultado = Telefone.Criar(telefone);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(11, resultado.Value!.Valor.Length);
    }

    [Fact]
    public void Deve_Remover_Formatacao_Do_Telefone()
    {
        var resultado = Telefone.Criar("(47) 99999-9999");

        Assert.Equal("47999999999", resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Retornar_Valor_No_ToString()
    {
        var resultado = Telefone.Criar("47999999999");

        Assert.Equal("47999999999", resultado.Value!.ToString());
    }

    [Fact]
    public void Telefone_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Telefone.Criar("47999999999");

        Assert.Empty(resultado.Notifications);
    }
}