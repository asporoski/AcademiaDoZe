// Amanda Soares Poroski

using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Fact]
    public void Deve_Criar_Logradouro_Valido()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "Brasil");

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(1, resultado.Value!.Id);
        Assert.Equal("88500000", resultado.Value.Cep.Valor);
        Assert.Equal("Rua Teste", resultado.Value.Nome);
        Assert.Equal("Centro", resultado.Value.Bairro);
        Assert.Equal("Lages", resultado.Value.Cidade);
        Assert.Equal("SC", resultado.Value.Estado);
        Assert.Equal("Brasil", resultado.Value.Pais);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Nome_For_Obrigatorio(string? nome)
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            nome!,
            "Centro",
            "Lages",
            "SC",
            "Brasil");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Bairro_For_Obrigatorio(string? bairro)
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            bairro!,
            "Lages",
            "SC",
            "Brasil");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "BAIRRO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Cidade_For_Obrigatoria(string? cidade)
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            cidade!,
            "SC",
            "Brasil");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "CIDADE_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Estado_For_Obrigatorio(string? estado)
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            estado!,
            "Brasil");

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ESTADO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Falhar_Quando_Pais_For_Obrigatorio(string? pais)
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            pais!);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "PAIS_OBRIGATORIO");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    public void Deve_Falhar_Quando_Cep_For_Invalido(string cep)
    {
        var resultado = Logradouro.Criar(
            1,
            cep,
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "Brasil");

        Assert.True(resultado.IsFailure);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Nome()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "   Rua   Teste   ",
            "Centro",
            "Lages",
            "SC",
            "Brasil");

        Assert.Equal(
            "Rua Teste",
            resultado.Value!.Nome);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Bairro()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "   Centro   ",
            "Lages",
            "SC",
            "Brasil");

        Assert.Equal(
            "Centro",
            resultado.Value!.Bairro);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Da_Cidade()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "   Lages   ",
            "SC",
            "Brasil");

        Assert.Equal(
            "Lages",
            resultado.Value!.Cidade);
    }

    [Fact]
    public void Deve_Converter_Estado_Para_Maiusculo()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "sc",
            "Brasil");

        Assert.Equal(
            "SC",
            resultado.Value!.Estado);
    }

    [Fact]
    public void Deve_Remover_Espacos_Do_Estado()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            " s c ",
            "Brasil");

        Assert.Equal(
            "SC",
            resultado.Value!.Estado);
    }

    [Fact]
    public void Deve_Limpar_Espacos_Do_Pais()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "   Brasil   ");

        Assert.Equal(
            "Brasil",
            resultado.Value!.Pais);
    }

    [Fact]
    public void Logradouro_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var resultado = Logradouro.Criar(
            1,
            "88500-000",
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "Brasil");

        Assert.Empty(resultado.Notifications);
    }

    [Fact]
    public void Deve_Acumular_Mais_De_Uma_Notificacao()
    {
        var resultado = Logradouro.Criar(
            1,
            "",
            "",
            "",
            "",
            "",
            "");

        Assert.True(resultado.IsFailure);
        Assert.True(resultado.Notifications.Count > 1);
    }
}