namespace Api.DTOs;

public sealed class AnimalSearchDto
{
    public string? Name { get; set; }
    public Guid? RacaId { get; set; }
    public Guid? CartaoVacinaId { get; set; }
    public DateOnly? DataNascimentoFrom { get; set; }
    public DateOnly? DataNascimentoTo { get; set; }
}

public sealed class AplicacaoVacinaSearchDto
{
    public Guid? VacinaId { get; set; }
    public Guid? CartaoVacinaId { get; set; }
    public DateOnly? DataAplicacaoFrom { get; set; }
    public DateOnly? DataAplicacaoTo { get; set; }
}

public sealed class CartaoVacinaSearchDto
{
    public Guid? VacinaId { get; set; }
    public DateOnly? DataAplicacaoFrom { get; set; }
    public DateOnly? DataAplicacaoTo { get; set; }
}

public sealed class EspecieSearchDto
{
    public string? Nome { get; set; }
    public string? NomeCientifico { get; set; }
}

public sealed class RacaSearchDto
{
    public string? Nome { get; set; }
    public Guid? EspecieId { get; set; }
    public string? EspecieNome { get; set; }
}

public sealed class VacinaSearchDto
{
    public string? Name { get; set; }
    public uint? ReaplicarEmXDiasMin { get; set; }
    public uint? ReaplicarEmXDiasMax { get; set; }
}
