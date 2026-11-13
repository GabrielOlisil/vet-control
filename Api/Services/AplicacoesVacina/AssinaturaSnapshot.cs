using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.DTOs;
using Api.Mappers;
using Api.Models;

namespace Api.Services.AplicacoesVacina;

public sealed record RegistroVacinaAssinado
{
    public int Versao { get; init; } = 2;
    public required AnimalDetailResponseDto Animal { get; init; }
    public required VacinaReadResponseDto Vacina { get; init; }
    public required JsonElement DadosAplicacao { get; init; }
    public Guid? AssinanteUserId { get; init; }
    public required string Assinante { get; init; }
    public required string Crmv { get; init; }
    public required string CrmvUf { get; init; }
    public DateTimeOffset AssinadaEm { get; init; }
}

public static class AssinaturaSnapshot
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Criar(AplicacaoVacina aplicacao) => Serializar(new RegistroVacinaAssinado
    {
        Animal = AnimalMapper.MapToResponse(aplicacao.Animal ?? throw new InvalidOperationException("Animal não carregado para assinatura.")),
        Vacina = VacinaMapper.MapToRead(aplicacao.Vacina ?? throw new InvalidOperationException("Vacina não carregada para assinatura.")),
        DadosAplicacao = Dados(aplicacao), AssinanteUserId = aplicacao.AssinanteUserId,
        Assinante = aplicacao.Assinante!, Crmv = aplicacao.AssinanteCrmv!, CrmvUf = aplicacao.AssinanteCrmvUf!,
        AssinadaEm = aplicacao.AssinadaEm!.Value
    });

    public static RegistroVacinaAssinado? Ler(AplicacaoVacina aplicacao)
    {
        if (aplicacao.VersaoAssinatura != 2 || string.IsNullOrWhiteSpace(aplicacao.SnapshotAssinado)) return null;
        try
        {
            var snapshot = JsonSerializer.Deserialize<RegistroVacinaAssinado>(aplicacao.SnapshotAssinado, Json);
            return snapshot is { Versao: 2, Animal: not null, Vacina: not null } &&
                   snapshot.DadosAplicacao.ValueKind == JsonValueKind.Object ? snapshot : null;
        }
        catch (JsonException) { return null; }
    }

    public static string Hash(string conteudo) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo))).ToLowerInvariant();

    public static string? Recalcular(AplicacaoVacina aplicacao)
    {
        var snapshot = Ler(aplicacao);
        if (snapshot is null || aplicacao.AssinadaEm is null) return null;
        return Hash(Serializar(snapshot with
        {
            DadosAplicacao = Dados(aplicacao), AssinanteUserId = aplicacao.AssinanteUserId,
            Assinante = aplicacao.Assinante ?? "", Crmv = aplicacao.AssinanteCrmv ?? "",
            CrmvUf = aplicacao.AssinanteCrmvUf ?? "", AssinadaEm = aplicacao.AssinadaEm.Value
        }));
    }

    private static string Serializar(RegistroVacinaAssinado snapshot) => JsonSerializer.Serialize(snapshot, Json);

    // Version 2 is immutable: introduce a new version rather than changing this schema.
    private static JsonElement Dados(AplicacaoVacina a) => JsonSerializer.SerializeToElement(new
    {
        a.Id, a.AnimalId, a.VacinaId, a.AnimalProtocoloVacinalId, a.ProtocoloVacinalDoseId,
        a.DataAplicacao, a.DataProximaDose, a.NumeroLote, a.DoseMl, a.DataFabricacao,
        a.DataValidade, a.LocalAplicacao, a.Observacoes, a.LaboratorioFabricante,
        a.Aplicador, a.AplicadorUserId, a.CreatedByUser, a.VeterinarioResponsavel,
        a.VeterinarioCrmv, a.VeterinarioCrmvUf, a.VeterinarioResponsavelUserId,
        a.RetificaAplicacaoVacinaId
    }, Json);

    public static AnimalReadResponseDto AnimalHistorico(RegistroVacinaAssinado s) => new()
    {
        Id = s.Animal.Id, Name = s.Animal.Name, Sexo = s.Animal.Sexo, Origem = s.Animal.Origem,
        DataNascimento = s.Animal.DataNascimento, DataNascimentoAproximada = s.Animal.DataNascimentoAproximada,
        LoteOuPasto = s.Animal.LoteOuPasto, Ativo = s.Animal.Ativo, IdentificadorPrincipal = s.Animal.IdentificadorPrincipal,
        Raca = s.Animal.Raca is null ? null : new RacaShortResponseDto { Id = s.Animal.Raca.Id, Nome = s.Animal.Raca.Nome }
    };
}
