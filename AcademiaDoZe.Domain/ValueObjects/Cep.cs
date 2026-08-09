// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cep
{
    public string Valor { get; }

    private Cep(string valor)
    {
        Valor = valor;
    }

    public static Result<Cep> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cep>.Failure("CEP", "CEP_OBRIGATORIO");

        var cepLimpo = NormalizadoService.LimparEDigitos(valor);

        if (cepLimpo.Length != 8)
            return Result<Cep>.Failure("CEP", "CEP_INVALIDO");

        return Result<Cep>.Success(new Cep(cepLimpo));
    }

    public override string ToString() => Valor;
}