using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs;

public class StringOrNumberConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        throw new JsonException($"Unexpected token type {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public sealed record AplicacaoVacinaCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid AnimalId { get; init; }

    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; init; }

    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; init; }

    [Required]
    public required string NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataFabricacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataValidade { get; init; }

    public string? LocalAplicacao { get; init; }

    public string? Observacoes { get; init; }

    public string? LaboratorioFabricante { get; init; }
}

public sealed record AplicacaoVacinaPatchDto
{
    private Guid? _animalProtocoloVacinalId;
    private Guid? _protocoloVacinalDoseId;
    private DateOnly? _dataProximaDose;
    private DateOnly? _dataFabricacao;
    private DateOnly? _dataValidade;
    private string? _doseMl;
    private string? _localAplicacao;
    private string? _observacoes;
    private string? _laboratorioFabricante;

    public Guid? AnimalId { get; init; }

    public Guid? VacinaId { get; init; }

    public Guid? AnimalProtocoloVacinalId
    {
        get => _animalProtocoloVacinalId;
        init
        {
            _animalProtocoloVacinalId = value;
            AnimalProtocoloVacinalIdWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool AnimalProtocoloVacinalIdWasProvided { get; private set; }

    public Guid? ProtocoloVacinalDoseId
    {
        get => _protocoloVacinalDoseId;
        init
        {
            _protocoloVacinalDoseId = value;
            ProtocoloVacinalDoseIdWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool ProtocoloVacinalDoseIdWasProvided { get; private set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly? DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose
    {
        get => _dataProximaDose;
        init
        {
            _dataProximaDose = value;
            DataProximaDoseWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DataProximaDoseWasProvided { get; private set; }

    public DateOnly? DataFabricacao
    {
        get => _dataFabricacao;
        init
        {
            _dataFabricacao = value;
            DataFabricacaoWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DataFabricacaoWasProvided { get; private set; }

    public DateOnly? DataValidade
    {
        get => _dataValidade;
        init
        {
            _dataValidade = value;
            DataValidadeWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DataValidadeWasProvided { get; private set; }

    public string? NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl
    {
        get => _doseMl;
        init
        {
            _doseMl = value;
            DoseMlWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DoseMlWasProvided { get; private set; }

    public string? LocalAplicacao
    {
        get => _localAplicacao;
        init
        {
            _localAplicacao = value;
            LocalAplicacaoWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool LocalAplicacaoWasProvided { get; private set; }

    public string? Observacoes
    {
        get => _observacoes;
        init
        {
            _observacoes = value;
            ObservacoesWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool ObservacoesWasProvided { get; private set; }

    public string? LaboratorioFabricante
    {
        get => _laboratorioFabricante;
        init
        {
            _laboratorioFabricante = value;
            LaboratorioFabricanteWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool LaboratorioFabricanteWasProvided { get; private set; }
}

public sealed record AplicacaoVacinaShortResponseDto
{
    public Guid Id { get; init; }

    public Guid VacinaId { get; init; }

    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }

    public VacinaShortResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public string NumeroLote { get; init; } = string.Empty;
}

public sealed record AplicacaoVacinaReadResponseDto
{
    public SituacaoRegistroVacina SituacaoRegistro { get; init; }
    public int VersaoAssinatura { get; init; }
    public bool DadosHistoricosPreservados { get; init; }
    public Guid? RetificaAplicacaoVacinaId { get; init; }
    public Guid? SubstituidaPorId { get; init; }
    public string? MotivoAlteracao { get; init; }
    public DateTimeOffset? AlteradaEm { get; init; }
    public Guid Id { get; init; }

    public Guid AnimalId { get; init; }

    public Guid VacinaId { get; init; }

    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }

    public AnimalShortResponseDto? Animal { get; init; }

    public VacinaShortResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public DateOnly? DataProximaDose { get; init; }

    public StatusComprovanteVacina StatusComprovante { get; init; } = StatusComprovanteVacina.PendenteAssinatura;

    public string NumeroLote { get; init; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    public DateOnly? DataFabricacao { get; init; }

    public DateOnly? DataValidade { get; init; }

    public string? LocalAplicacao { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? VeterinarioCrmv { get; init; }

    public string? VeterinarioCrmvUf { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }

    public DateTimeOffset? AssinadaEm { get; init; }

    public string? Assinante { get; init; }

    public string? AssinanteCrmv { get; init; }

    public string? AssinanteCrmvUf { get; init; }

    public string? HashAssinatura { get; init; }

    public string? MetodoAssinatura { get; init; }
}

public sealed record AplicacaoVacinaDetailResponseDto
{
    public SituacaoRegistroVacina SituacaoRegistro { get; init; }
    public int VersaoAssinatura { get; init; }
    public bool DadosHistoricosPreservados { get; init; }
    public Guid? AplicadorUserId { get; init; }
    public Guid? VeterinarioResponsavelUserId { get; init; }
    public Guid? AssinanteUserId { get; init; }
    public Guid? RetificaAplicacaoVacinaId { get; init; }
    public Guid? SubstituidaPorId { get; init; }
    public string? MotivoAlteracao { get; init; }
    public DateTimeOffset? AlteradaEm { get; init; }
    public Guid Id { get; init; }

    public Guid AnimalId { get; init; }

    public Guid VacinaId { get; init; }

    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }

    public AnimalReadResponseDto? Animal { get; init; }

    public VacinaReadResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public DateOnly? DataProximaDose { get; init; }

    public required string NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    public string? Observacoes { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }

    public DateOnly? DataFabricacao { get; init; }

    public DateOnly? DataValidade { get; init; }

    public string? LocalAplicacao { get; init; }

    public string? VeterinarioCrmv { get; init; }

    public string? VeterinarioCrmvUf { get; init; }

    public StatusComprovanteVacina StatusComprovante { get; init; } = StatusComprovanteVacina.PendenteAssinatura;

    public DateTimeOffset? AssinadaEm { get; init; }

    public string? Assinante { get; init; }

    public string? AssinanteCrmv { get; init; }

    public string? AssinanteCrmvUf { get; init; }

    public string? HashAssinatura { get; init; }

    public string? MetodoAssinatura { get; init; }

    public List<AssinaturaAplicacaoVacinaResponseDto> Assinaturas { get; init; } = [];

    public DateTimeOffset CreationDateTime { get; init; }

    public DateTimeOffset? LastModificationDateTime { get; init; }

    public DateTimeOffset CriadoEm => CreationDateTime;

    public DateTimeOffset? AtualizadoEm => LastModificationDateTime;
}

public sealed record AssinaturaAplicacaoVacinaResponseDto
{
    public Guid Id { get; init; }
    public Guid? AssinanteUserId { get; init; }
    public string TipoEvento { get; init; } = string.Empty;
    public string NomeAssinante { get; init; } = string.Empty;
    public string Crmv { get; init; } = string.Empty;
    public string CrmvUf { get; init; } = string.Empty;
    public DateTimeOffset DataHora { get; init; }
    public string HashConteudo { get; init; } = string.Empty;
    public string Metodo { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
}

public sealed record AplicacaoVacinaAssinaturaDto
{
    public string? Observacoes { get; init; }
}

public sealed record CancelamentoVacinaDto
{
    [Required, StringLength(1000, MinimumLength = 3)]
    public required string Motivo { get; init; }
}

public sealed record RetificacaoVacinaDto
{
    [Required, StringLength(1000, MinimumLength = 3)]
    public required string Motivo { get; init; }
    [Required]
    public required AplicacaoVacinaPatchDto Alteracoes { get; init; }
}

public sealed record AssinaturaValidacaoResponseDto
{
    public Guid AplicacaoVacinaId { get; init; }
    public bool Assinada { get; init; }
    public bool Integra { get; init; }
    public bool AuditoriaIntegra { get; init; }
    public DateTimeOffset? AssinadaEm { get; init; }
    public string? HashArmazenado { get; init; }
    public string? HashCalculado { get; init; }
    public string Mensagem { get; init; } = string.Empty;
}

public sealed record ValidacaoPublicaVacinaResponseDto
{
    public SituacaoRegistroVacina SituacaoRegistro { get; init; }
    public bool RegistroVigente { get; init; }
    public bool DadosHistoricosPreservados { get; init; }
    public Guid? SubstituidaPorId { get; init; }
    public Guid? RetificaAplicacaoVacinaId { get; init; }
    public string? MotivoAlteracao { get; init; }
    public bool Encontrado { get; init; }
    public string Mensagem { get; init; } = string.Empty;
    public bool Assinada { get; init; }
    public bool AssinaturaIntegra { get; init; }
    public string? VacinaNome { get; init; }
    public string? AnimalIdentificacao { get; init; }
    public DateOnly? DataAplicacao { get; init; }
    public string? NumeroLote { get; init; }
    public string? VeterinarioResponsavel { get; init; }
    public string? VeterinarioCrmv { get; init; }
    public string? VeterinarioCrmvUf { get; init; }
    public string? Assinante { get; init; }
    public string? AssinanteCrmv { get; init; }
    public string? AssinanteCrmvUf { get; init; }
    public DateTimeOffset? AssinadaEm { get; init; }
}

public sealed record ComprovanteVacinasDadosResponseDto
{
    public required AnimalDetailResponseDto Animal { get; init; }
    public List<AplicacaoVacinaDetailResponseDto> AplicacoesVacina { get; init; } = [];
    public DateTimeOffset GeradoEm { get; init; }
}

public sealed record ItemAnimalLoteDto
{
    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }
    [Required]
    [NonEmptyGuid]
    public Guid AnimalId { get; init; }

}

public sealed record AplicacaoVacinaLoteCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; init; }

    [Required]
    public required string NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataFabricacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataValidade { get; init; }

    public string? LocalAplicacao { get; init; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; init; }

    public string? Observacoes { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? VeterinarioCrmv { get; init; }

    public string? VeterinarioCrmvUf { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "A lista de animais deve conter pelo menos um animal.")]
    public required List<ItemAnimalLoteDto> Animais { get; init; }
}

