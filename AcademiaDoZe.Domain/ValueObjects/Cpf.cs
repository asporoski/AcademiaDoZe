// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("CPF", "CPF_OBRIGATORIO");

        var cpfLimpo = NormalizadoService.LimparEDigitos(valor);

        if (cpfLimpo.Length != 11)
            return Result<Cpf>.Failure("CPF", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(cpfLimpo));
    }

    public override string ToString() => Valor;
}