namespace Api.Models;

public class CartaoVacina
{
    public Guid Id { get; set; }

    public List<AplicacaoVacina> VacinasAplicadas { get; set; } = [];
}
