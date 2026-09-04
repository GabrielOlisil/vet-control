namespace Api.Models;

public class Especie
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public required string NomeCientifico { get; set; }

    public List<Raca> Racas { get; set; } = [];
}
