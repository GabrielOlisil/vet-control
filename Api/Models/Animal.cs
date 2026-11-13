
using Api.Models.Enums;

namespace Api.Models;

public class Animal : IAuditedEntity
{
    public Guid Id { get; set; }
    public uint Version { get; set; }
    public string? Name { get; set; }

    public SexoAnimal Sexo { get; set; } = SexoAnimal.Indefinido;
    public Guid? RacaId { get; set; }
    public DateOnly DataNascimento { get; set; }
    public bool DataNascimentoAproximada { get; set; }

    public OrigemAnimal OrigemAnimal { get; set; } = OrigemAnimal.Externo;

    public string? LoteOuPasto { get; set; }
    public Guid? AmbienteId { get; set; }
    public Ambiente? Ambiente { get; set; }

    public bool Ativo { get; set; } = true;


    public Raca? Raca { get; set; }
    public List<AplicacaoVacina> VacinasAplicadas { get; set; } = [];
    public List<IdentificadorAnimal> Identificadores { get; set; } = [];
    public List<AnimalProtocoloVacinal> ProtocolosVacinais { get; set; } = [];
    public List<MovimentacaoAnimal> Movimentacoes { get; set; } = [];


    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
