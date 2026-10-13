namespace Api.Models;

public class AssinaturaAplicacaoVacina
{
    public Guid Id { get; set; }
    public Guid AplicacaoVacinaId { get; set; }

    public string TipoEvento { get; set; } = "Assinatura";
    public string NomeAssinante { get; set; } = string.Empty;
    public string Crmv { get; set; } = string.Empty;
    public string CrmvUf { get; set; } = string.Empty;
    public DateTimeOffset DataHora { get; set; }
    public string HashConteudo { get; set; } = string.Empty;
    public string Metodo { get; set; } = "InternaAuditavel";
    public string? Observacoes { get; set; }

    public AplicacaoVacina? AplicacaoVacina { get; set; }
}
