namespace Api.DTOs;

public sealed class AnimalSearchDto
{
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


public sealed class RacaSearchDto
{
    public Guid? EspecieId { get; set; }
}

public sealed class VacinaSearchDto
{
    public uint? ReaplicarEmXDiasMin { get; set; }
    public uint? ReaplicarEmXDiasMax { get; set; }
}
