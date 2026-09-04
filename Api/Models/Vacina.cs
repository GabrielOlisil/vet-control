namespace Api.Models;

public class Vacina
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public uint ReaplicarEmXDias { get; set; }
}
