using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.DTOs.Vacinas;

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

public sealed class AplicacaoVacinaReadResponseDto
{
    public Guid Id { get; set; }

    public required VacinaShortResponseDto Vacina { get; set; }

    public Guid? CartaoVacinaId { get; set; } //voltar

    public DateOnly DataAplicacao { get; set; }
}

public sealed class AplicacaoVacinaShortResponseDto
{
    public Guid Id { get; set; }
    public required VacinaShortResponseDto Vacina { get; set; }
    public DateOnly DataAplicacao { get; set; }

}


public sealed class AplicacaoVacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public required string VacinaName { get; set; }
    public uint ReaplicarEmXDias { get; set; }

    public Guid? CartaoVacinaId { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public DateTime ProximaAplicacao
    {
        get
        {
            return DataAplicacao.ToDateTime(new TimeOnly(0, 0))
            .AddDays(ReaplicarEmXDias);
        }
    }
}

