namespace Api.Models;

public class CartaoVacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public List<AplicacaoVacina> VacinasAplicadas { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
