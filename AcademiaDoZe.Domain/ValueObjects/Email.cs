// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Result<Email> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Email>.Failure("EMAIL", "EMAIL_OBRIGATORIO");

        var emailLimpo = NormalizadoService.LimparEspacos(valor);

        var partes = emailLimpo.Split('@');

        if (partes.Length != 2 ||
            string.IsNullOrWhiteSpace(partes[0]) ||
            string.IsNullOrWhiteSpace(partes[1]))
            return Result<Email>.Failure("EMAIL", "EMAIL_INVALIDO");

        var dominio = partes[1];

        if (!dominio.Contains('.') ||
            dominio.StartsWith('.') ||
            dominio.EndsWith('.'))
            return Result<Email>.Failure("EMAIL", "EMAIL_INVALIDO");

        return Result<Email>.Success(new Email(emailLimpo));
    }

    public override string ToString() => Valor;
}