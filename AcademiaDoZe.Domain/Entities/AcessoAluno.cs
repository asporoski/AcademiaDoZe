
// Amanda Soares Poroski

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }

    public DateTime DataHora { get; private set; }

    public DateTime? DataHoraSaida { get; private set; }

    public DateTime DataHoraEntrada => DataHora;

    public bool EstaNaAcademia =>
        DataHoraSaida == null;

    public TimeSpan? TempoPermanencia =>
        DataHoraSaida.HasValue
            ? DataHoraSaida.Value - DataHora
            : null;

    private AcessoAluno(
        int id,
        int alunoId,
        DateTime dataHora,
        DateTime? dataHoraSaida = null)
        : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoAluno> Criar(
        int id,
        Aluno aluno,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
        {
            notifications.Add(
                new Notification(
                    "Aluno",
                    "ALUNO_INVALIDO"));
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
            return Result<AcessoAluno>.Failure(notifications);

        return Result<AcessoAluno>.Success(
            new AcessoAluno(
                id,
                aluno!.Id,
                dataHora));
    }

    // Reconstrói um acesso já salvo no banco.
    public static AcessoAluno Reconstituir(
        int id,
        int alunoId,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida)
    {
        if (id <= 0 || alunoId <= 0)
            throw new ArgumentException(
                "Identificação do acesso inválida.");

        if (dataHoraEntrada == default)
            throw new ArgumentException(
                "Data de entrada inválida.");

        if (dataHoraSaida.HasValue &&
            dataHoraSaida.Value < dataHoraEntrada)
            throw new ArgumentException(
                "Saída anterior à entrada.");

        return new AcessoAluno(
            id,
            alunoId,
            dataHoraEntrada,
            dataHoraSaida);
    }

    public Result<AcessoAluno> RegistrarSaida(
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
            return Result<AcessoAluno>.Failure(notifications);

        DataHoraSaida = dataHoraSaida;

        return Result<AcessoAluno>.Success(this);
    }
}
