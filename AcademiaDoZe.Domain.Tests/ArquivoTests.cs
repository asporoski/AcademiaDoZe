// Amanda Soares Poroski

using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ArquivoTests
{
    [Fact]
    public void Deve_Falhar_Quando_Arquivo_Nulo()
    {
        var resultado = Arquivo.Criar(null!);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact]
    public void Deve_Criar_Arquivo_Vazio()
    {
        var conteudo = Array.Empty<byte>();

        var resultado = Arquivo.Criar(conteudo);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Empty(resultado.Value!.Conteudo);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1024)]
    [InlineData(5000)]
    public void Deve_Criar_Arquivo_Com_Tamanho_Valido(int tamanho)
    {
        var conteudo = new byte[tamanho];

        var resultado = Arquivo.Criar(conteudo);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(tamanho, resultado.Value!.Conteudo.Length);
    }

    [Fact]
    public void Deve_Criar_Arquivo_Com_Exatos_15MB()
    {
        var conteudo = new byte[15 * 1024 * 1024];

        var resultado = Arquivo.Criar(conteudo);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Deve_Falhar_Quando_Arquivo_Ultrapassar_15MB()
    {
        var conteudo = new byte[(15 * 1024 * 1024) + 1];

        var resultado = Arquivo.Criar(conteudo);

        Assert.True(resultado.IsFailure);

        Assert.Contains(
            resultado.Notifications,
            n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }

    [Fact]
    public void Arquivo_Valido_Nao_Deve_Possuir_Notificacoes()
    {
        var conteudo = new byte[100];

        var resultado = Arquivo.Criar(conteudo);

        Assert.Empty(resultado.Notifications);
    }

    [Fact]
    public void Deve_Manter_O_Mesmo_Conteudo()
    {
        byte[] conteudo = [1, 2, 3, 4, 5];

        var resultado = Arquivo.Criar(conteudo);

        Assert.Equal(
            conteudo,
            resultado.Value!.Conteudo);
    }
}