// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
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
        return Arquivo.Criar(
            Array.Empty<byte>()).Value!;
    }

    private static Aluno CriarAlunoValido()
    {
        return Aluno.Criar(
            1,
            "Amanda Poroski",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "Bloco A",
            "Amanda1",
            CriarArquivoValido()).Value!;
    }

    [Fact]
    public void Deve_Criar_Aluno_Valido()
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda Poroski",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "Bloco A",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(1, resultado.Value!.Id);
        Assert.Equal(
            "Amanda Poroski",
            resultado.Value.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Nome_For_Obrigatorio(
        string? nome)
    {
        var resultado = Aluno.Criar(
            1,
            nome!,
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Falhar_Quando_DataNascimento_For_Obrigatoria()
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            default,
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem ==
                 "DATA_NASCIMENTO_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Falhar_Quando_Aluno_Tiver_Menos_De_12_Anos()
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-11)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem ==
                 "DATA_NASCIMENTO_MINIMA_INVALIDA");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Quando_Cpf_For_Invalido(
        string cpf)
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            cpf,
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("4999999999")]
    [InlineData("499999999999")]
    public void Deve_Falhar_Quando_Telefone_For_Invalido(
        string telefone)
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            telefone,
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("amanda")]
    [InlineData("@gmail.com")]
    [InlineData("amanda@gmail")]
    [InlineData("amanda@.com")]
    public void Deve_Falhar_Quando_Email_For_Invalido(
        string email)
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            email,
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("abcdef")]
    [InlineData("123456")]
    public void Deve_Falhar_Quando_Senha_For_Invalida(
        string senha)
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            senha,
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);
    }

    [Fact]
    public void Deve_Falhar_Quando_Logradouro_For_Nulo()
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            null!,
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem ==
                 "LOGRADOURO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Numero_For_Obrigatorio(
        string? numero)
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            numero!,
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "NUMERO_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Nome()
    {
        var resultado = Aluno.Criar(
            1,
            "   Amanda   Poroski   ",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.Equal(
            "Amanda Poroski",
            resultado.Value!.Nome);
    }

    [Fact]
    public void Deve_Armazenar_Dados_Do_Aluno()
    {
        var aluno = CriarAlunoValido();

        Assert.Equal(
            "12345678901",
            aluno.Cpf.Valor);

        Assert.Equal(
            "49999999999",
            aluno.Telefone.Valor);

        Assert.Equal(
            "amanda@gmail.com",
            aluno.Email.Valor);

        Assert.Equal(
            "Amanda1",
            aluno.Senha.Valor);
    }

    [Fact]
    public void Aluno_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Aluno.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido());

        Assert.Empty(resultado.Notifications);
    }

    [Fact]
    public void Deve_Acumular_Varias_Notificacoes()
    {
        var resultado = Aluno.Criar(
            1,
            "",
            "",
            default,
            "",
            "",
            null!,
            "",
            "",
            "",
            CriarArquivoValido());

        Assert.True(resultado.IsFailure);
        Assert.True(
            resultado.Notifications.Count > 1);
    }
}