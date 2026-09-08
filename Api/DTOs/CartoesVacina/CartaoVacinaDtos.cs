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

public sealed class CartaoVacinaReadResponseDto
{
    public Guid Id { get; set; }

    public List<AplicacaoVacinaShortResponseDto> VacinasAplicadas { get; set; } = [];
}

public sealed class CartaoVacinaShortResponseDto
{
    public Guid Id { get; set; }

    public uint NumVacinas { get; set; }
}

public sealed class CartaoVacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public List<AplicacaoVacinaShortResponseDto> VacinasAplicadas { get; set; } = [];
}