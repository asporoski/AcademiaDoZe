// Amanda Soares Poroski

namespace AcademiaDoZe.Domain.Entities;

public class Logradouro
{
    public string Nome { get; private set; }

    public Logradouro(string nome)
    {
        Nome = nome;
    }
}