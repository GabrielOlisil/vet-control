using Api.DTOs.AplicacoesVacina;

namespace Api.DTOs.CartoesVacina;

public sealed class CartaoVacinaCreateDto
{
    public List<Guid>? VacinasAplicadasIds { get; set; }
}

public sealed class CartaoVacinaPatchDto
{
    public List<Guid>? VacinasAplicadasIds { get; set; }
}

public sealed class CartaoVacinaReadDto
{
    public Guid Id { get; set; }

    public List<AplicacaoVacinaResumoDto> VacinasAplicadas { get; set; } = [];
}

public sealed class CartaoVacinaResponseDto
{
    public Guid Id { get; set; }

    public List<AplicacaoVacinaResponseDto> VacinasAplicadas { get; set; } = [];
}

public sealed class AplicacaoVacinaResumoDto
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public required string VacinaName { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public DateOnly DataAplicacao { get; set; }
}