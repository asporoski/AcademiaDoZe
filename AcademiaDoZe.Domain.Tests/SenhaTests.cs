// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class SenhaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Senha_Obrigatoria(string? senha)
    {
        var resultado = Senha.Criar(senha!);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "SENHA_OBRIGATORIO");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abcde")]
    [InlineData("12345")]
    [InlineData("abcdef")]
    [InlineData("123456")]
    [InlineData("senha123")]
    public void Deve_Falhar_Quando_Senha_Formato_Invalido(
        string senha)
    {
        var resultado = Senha.Criar(senha);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "SENHA_FORMATO");
    }

    [Theory]
    [InlineData("Amanda1")]
    [InlineData("Senha123")]
    [InlineData("TesteA")]
    [InlineData("ABCDEF")]
    [InlineData("Abcdef")]
    public void Deve_Criar_Senha_Valida(string senha)
    {
        var resultado = Senha.Criar(senha);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(senha, resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Remover_Espacos_Externos()
    {
        var resultado = Senha.Criar(
            "   Amanda1   ");

        Assert.True(resultado.IsSuccess);
        Assert.Equal(
            "Amanda1",
            resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Retornar_Valor_No_ToString()
    {
        var resultado = Senha.Criar("Amanda1");

        Assert.Equal(
            "Amanda1",
            resultado.Value!.ToString());
    }

    [Fact]
    public void Senha_Valida_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Senha.Criar("Amanda1");

        Assert.Empty(resultado.Notifications);
    }
}