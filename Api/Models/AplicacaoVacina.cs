namespace Api.Models;

public class AplicacaoVacina
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public Vacina? Vacina { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public CartaoVacina? CartaoVacina { get; set; }

    public DateOnly DataAplicacao { get; set; }
}
