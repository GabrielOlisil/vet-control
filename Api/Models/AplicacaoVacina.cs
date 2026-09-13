using Api.Models.Enums;

namespace Api.Models;

public class AplicacaoVacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public Guid AnimalId { get; set; }

    public DateOnly DataAplicacao { get; set; }
    public DateOnly? DataProximaDose { get; set; }

    public bool CicloFinalizado { get; set; } = true;

    public StatusComprovanteVacina StatusComprovante { get; set; } = StatusComprovanteVacina.NaoEmitido;

    public string? ComprovanteDocumentoPath { get; set; }

    public required string NumeroLote { get; set; }

    public string? DoseMl { get; set; }

    public string? Observacoes { get; set; }

    public string? VeterinarioResponsavel { get; set; }
    public string? Aplicador { get; set; }
    public string? LaboratorioFabricante { get; set; }

    public Vacina? Vacina { get; set; }
    public Animal? Animal { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
