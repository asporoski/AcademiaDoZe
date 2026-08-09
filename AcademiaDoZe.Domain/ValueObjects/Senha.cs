// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        return Result<Senha>.Success(new Senha(valor));
    }

    public override string ToString() => Valor;
}