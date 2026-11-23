
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public int ColaboradorId { get; private set; }

    public DateTime DataHora { get; private set; }

    public DateTime? DataHoraSaida { get; private set; }

    public DateTime DataHoraEntrada => DataHora;

    public bool EstaNaAcademia =>
        DataHoraSaida == null;

    public TimeSpan? TempoPermanencia =>
        DataHoraSaida.HasValue
            ? DataHoraSaida.Value - DataHora
            : null;

    private AcessoColaborador(
        int id,
        int colaboradorId,
        DateTime dataHora,
        DateTime? dataHoraSaida = null)
        : base(id)
    {
        ColaboradorId = colaboradorId;
        DataHora = dataHora;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoColaborador> Criar(
        int id,
        Colaborador colaborador,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
        {
            notifications.Add(
                new Notification(
                    "Colaborador",
                    "COLABORADOR_INVALIDO"));
        }

        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
        {
            notifications.Add(
                new Notification(
                    "DataHora",
                    "DATA_HORA_INTERVALO_INVALIDO"));
        }

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        return Result<AcessoColaborador>.Success(
            new AcessoColaborador(
                id,
                colaborador!.Id,
                dataHora));
    }

    public static AcessoColaborador Reconstituir(
        int id,
        int colaboradorId,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida)
    {
        if (id <= 0 || colaboradorId <= 0)
            throw new ArgumentException(
                "Identificação do acesso inválida.");

        if (dataHoraEntrada == default)
            throw new ArgumentException(
                "Data de entrada inválida.");

        if (dataHoraSaida.HasValue &&
            dataHoraSaida.Value < dataHoraEntrada)
            throw new ArgumentException(
                "Saída anterior à entrada.");

        return new AcessoColaborador(
            id,
            colaboradorId,
            dataHoraEntrada,
            dataHoraSaida);
    }

    public Result<AcessoColaborador> RegistrarSaida(
        DateTime dataHoraSaida)
    {
        var notifications = new List<Notification>();

        if (DataHoraSaida.HasValue)
        {
            notifications.Add(
                new Notification(
                    "DataHoraSaida",
                    "SAIDA_JA_REGISTRADA"));
        }

        if (dataHoraSaida < DataHora)
        {
            notifications.Add(
                new Notification(
                    "DataHoraSaida",
                    "SAIDA_ANTERIOR_ENTRADA"));
        }

        if (dataHoraSaida.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHoraSaida.TimeOfDay > new TimeSpan(22, 0, 0))
        {
            notifications.Add(
                new Notification(
                    "DataHoraSaida",
                    "DATA_HORA_INTERVALO_INVALIDO"));
        }

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        DataHoraSaida = dataHoraSaida;

        return Result<AcessoColaborador>.Success(this);
    }
}
