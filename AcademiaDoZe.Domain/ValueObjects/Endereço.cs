// Amanda Soares Poroski

namespace AcademiaDoZe.Domain.ValueObjects;

public record Endereco
{
    public Cep Cep { get; }
    public string Pais { get; }
    public string Estado { get; }
    public string Cidade { get; }
    public string Bairro { get; }
    public string Logradouro { get; }
    public string Numero { get; }
    public string Complemento { get; }

    public Endereco(
        Cep cep,
        string pais,
        string estado,
        string cidade,
        string bairro,
        string logradouro,
        string numero,
        string complemento)
    {
        Cep = cep;
        Pais = pais;
        Estado = estado;
        Cidade = cidade;
        Bairro = bairro;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
    }
}