using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs.AplicacoesVacina;

public sealed class AplicacaoVacinaCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AplicacaoVacinaPatchDto
{
    public Guid? VacinaId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataAplicacao { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AplicacaoVacinaReadDto
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public required string VacinaName { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public DateOnly DataAplicacao { get; set; }
}

public sealed class AplicacaoVacinaResponseDto
{
    public Guid Id { get; set; }

    public required VacinaInfoDto Vacina { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public DateTime ProximaAplicacao
    {
        get
        {
            return DataAplicacao.ToDateTime(new TimeOnly(0, 0)).AddDays(Vacina.ReaplicarEmXDias);
        }
    }
}

public sealed class VacinaInfoDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public uint ReaplicarEmXDias { get; set; }
}