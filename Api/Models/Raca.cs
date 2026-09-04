namespace Api.Models;

public class Raca
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public Guid EspecieId { get; set; }

    public Especie Especie { get; set; }

    public List<Animal> Animais { get; set; } = [];
}
