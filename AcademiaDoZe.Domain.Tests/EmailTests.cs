// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Email_Vazio(string? email)
    {
        var resultado = Email.Criar(email!);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory]
    [InlineData("amanda")]
    [InlineData("amanda@")]
    [InlineData("@gmail.com")]
    [InlineData("amanda@gmail")]
    [InlineData("amanda@.com")]
    [InlineData("amanda@gmail.")]
    [InlineData("amanda@@gmail.com")]
    [InlineData("amanda@gmail..com")]
    public void Deve_Falhar_Quando_Email_Formato_Invalido(string email)
    {
        var resultado = Email.Criar(email);

        Assert.True(resultado.IsFailure);
        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory]
    [InlineData("amanda@gmail.com")]
    [InlineData("teste@hotmail.com")]
    [InlineData("usuario@empresa.com.br")]
    [InlineData("aluno@academia.com")]
    public void Deve_Criar_Email_Valido(string email)
    {
        var resultado = Email.Criar(email);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(email, resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Remover_Espacos_Externos()
    {
        var resultado = Email.Criar(
            "   amanda@gmail.com   ");

        Assert.True(resultado.IsSuccess);
        Assert.Equal(
            "amanda@gmail.com",
            resultado.Value!.Valor);
    }

    [Fact]
    public void Deve_Retornar_Valor_No_ToString()
    {
        var resultado = Email.Criar(
            "amanda@gmail.com");

        Assert.Equal(
            "amanda@gmail.com",
            resultado.Value!.ToString());
    }

    [Fact]
    public void Email_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Email.Criar(
            "amanda@gmail.com");

        Assert.Empty(resultado.Notifications);
    }
}