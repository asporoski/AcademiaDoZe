// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Cpf_Obrigatorio(string? cpf)
    {
        var resultado = Cpf.Criar(cpf!);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("123.456")]
    public void Deve_Falhar_Quando_Cpf_Nao_Tiver_Onze_Digitos(string cpf)
    {
        var resultado = Cpf.Criar(cpf);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("123.456.789-01")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Deve_Criar_Cpf_Com_Onze_Digitos(string cpf)
    {
        var resultado = Cpf.Criar(cpf);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(11, resultado.Value!.Valor.Length);
    }

    [Fact]
    public void Deve_Remover_Pontos_E_Hifen()
    {
        var resultado = Cpf.Criar("123.456.789-01");

        Assert.Equal("12345678901", resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Retornar_Valor_No_ToString()
    {
        var resultado = Cpf.Criar("12345678901");

        Assert.Equal("12345678901", resultado.Value!.ToString());
    }

    [Fact]
    public void Cpf_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Cpf.Criar("12345678901");

        Assert.Empty(resultado.Notifications);
    }
}