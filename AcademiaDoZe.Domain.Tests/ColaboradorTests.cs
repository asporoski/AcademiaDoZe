// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
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

    private static Colaborador CriarColaboradorValido()
    {
        return Colaborador.Criar(
            1,
            "Amanda Poroski",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT).Value!;
    }

    [Fact]
    public void Deve_Criar_Colaborador_Valido()
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda Poroski",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(1, resultado.Value!.Id);
        Assert.Equal("Amanda Poroski", resultado.Value.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Nome_For_Obrigatorio(string? nome)
    {
        var resultado = Colaborador.Criar(
            1,
            nome!,
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Falhar_Quando_DataNascimento_For_Obrigatoria()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_NASCIMENTO_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Falhar_Quando_Idade_For_Menor_De_12_Anos()
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-11)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_NASCIMENTO_MINIMA_INVALIDA");
    }

    [Fact]
    public void Deve_Falhar_Quando_DataAdmissao_For_Obrigatoria()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            default,
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_ADMISSAO_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Falhar_Quando_DataAdmissao_For_Futura()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "DATA_ADMISSAO_MAIOR_QUE_ATUAL");
    }

    [Fact]
    public void Deve_Aceitar_DataAdmissao_De_Hoje()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_Quando_Tipo_For_Invalido()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            (ColaboradorTipo)999,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Fact]
    public void Deve_Falhar_Quando_Vinculo_For_Invalido()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            (ColaboradorVinculo)999);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    }

    [Fact]
    public void Administrador_Deve_Ser_CLT()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Administrador,
            ColaboradorVinculo.Estagio);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    }

    [Fact]
    public void Deve_Criar_Administrador_CLT()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Administrador,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(
            ColaboradorTipo.Administrador,
            resultado.Value!.Tipo);
        Assert.Equal(
            ColaboradorVinculo.CLT,
            resultado.Value.Vinculo);
    }

    [Theory]
    [InlineData(ColaboradorTipo.Atendente)]
    [InlineData(ColaboradorTipo.Instrutor)]
    public void Deve_Criar_Outros_Tipos_Com_Vinculo_Estagio(
        ColaboradorTipo tipo)
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            tipo,
            ColaboradorVinculo.Estagio);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(tipo, resultado.Value!.Tipo);
        Assert.Equal(
            ColaboradorVinculo.Estagio,
            resultado.Value.Vinculo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Quando_Cpf_For_Invalido(string cpf)
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda",
            cpf,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("4999999999")]
    [InlineData("499999999999")]
    public void Deve_Falhar_Quando_Telefone_For_Invalido(string telefone)
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            telefone,
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("amanda")]
    [InlineData("@gmail.com")]
    [InlineData("amanda@gmail")]
    public void Deve_Falhar_Quando_Email_For_Invalido(string email)
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            email,
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("abcdef")]
    [InlineData("123456")]
    public void Deve_Falhar_Quando_Senha_For_Invalida(string senha)
    {
        var resultado = Colaborador.Criar(
            1,
            "Amanda",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            senha,
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.True(resultado.IsFailure);
    }

    [Fact]
    public void Deve_Armazenar_Dados_Do_Colaborador()
    {
        var colaborador = CriarColaboradorValido();

        Assert.Equal(
            ColaboradorTipo.Atendente,
            colaborador.Tipo);

        Assert.Equal(
            ColaboradorVinculo.CLT,
            colaborador.Vinculo);

        Assert.Equal(
            "12345678901",
            colaborador.Cpf.Valor);

        Assert.Equal(
            "49999999999",
            colaborador.Telefone.Valor);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Nome()
    {
        var resultado = Colaborador.Criar(
            1,
            "   Amanda   Poroski   ",
            "12345678901",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "49999999999",
            "amanda@gmail.com",
            CriarLogradouroValido(),
            "10",
            "",
            "Amanda1",
            CriarArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT);

        Assert.Equal(
            "Amanda Poroski",
            resultado.Value!.Nome);
    }

    [Fact]
    public void Colaborador_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var colaborador = CriarColaboradorValido();

        Assert.NotNull(colaborador);
    }

    [Fact]
    public void Deve_Acumular_Varias_Notificacoes()
    {
        var resultado = Colaborador.Criar(
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
            CriarArquivoValido(),
            default,
            (ColaboradorTipo)999,
            (ColaboradorVinculo)999);

        Assert.True(resultado.IsFailure);
        Assert.True(resultado.Notifications.Count > 1);
    }
}