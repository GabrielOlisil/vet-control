namespace Api.Models;

public class AplicacaoVacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public Vacina? Vacina { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public CartaoVacina? CartaoVacina { get; set; }

    public DateOnly DataAplicacao { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
