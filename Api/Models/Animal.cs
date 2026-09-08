
namespace Api.Models;

public class Animal : IAuditedEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid? RacaId { get; set; }

    public Raca? Raca { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public CartaoVacina? CartaoVacina { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string? PictureUpload { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
