using Api.Models.Enums;

namespace Api.Models;

public class AplicacaoVacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public Guid AnimalId { get; set; }
    public Guid? AnimalProtocoloVacinalId { get; set; }
    public Guid? ProtocoloVacinalDoseId { get; set; }

    public DateOnly DataAplicacao { get; set; }
    public DateOnly? DataProximaDose { get; set; }

    public bool CicloFinalizado { get; set; } = true;

    public StatusComprovanteVacina StatusComprovante { get; set; } = StatusComprovanteVacina.PendenteAssinatura;

    public required string NumeroLote { get; set; }

    public string? DoseMl { get; set; }

    public DateOnly? DataFabricacao { get; set; }
    public DateOnly? DataValidade { get; set; }
    public string? LocalAplicacao { get; set; }

    public string? Observacoes { get; set; }

    public string? VeterinarioResponsavel { get; set; }
    public string? VeterinarioCrmv { get; set; }
    public string? VeterinarioCrmvUf { get; set; }
    public string? Aplicador { get; set; }
    public string? LaboratorioFabricante { get; set; }

    public DateTimeOffset? AssinadaEm { get; set; }
    public string? Assinante { get; set; }
    public string? AssinanteCrmv { get; set; }
    public string? AssinanteCrmvUf { get; set; }
    public string? HashAssinatura { get; set; }
    public string? MetodoAssinatura { get; set; }

    public Vacina? Vacina { get; set; }
    public Animal? Animal { get; set; }
    public AnimalProtocoloVacinal? AnimalProtocoloVacinal { get; set; }
    public ProtocoloVacinalDose? ProtocoloVacinalDose { get; set; }
    public List<AssinaturaAplicacaoVacina> Assinaturas { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
