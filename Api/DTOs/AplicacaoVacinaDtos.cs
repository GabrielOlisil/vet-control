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

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; init; }

    public bool? CicloFinalizado { get; init; } = true;

    [Required]
    public required string NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    public string? Observacoes { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }
}

public sealed record AplicacaoVacinaPatchDto
{
    public Guid? AnimalId { get; init; }

    public Guid? VacinaId { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; init; }

    public bool? CicloFinalizado { get; init; }

    public StatusComprovanteVacina? StatusComprovante { get; init; }

    public string? ComprovanteDocumentoPath { get; init; }

    public string? NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    public string? Observacoes { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }
}

public sealed record AplicacaoVacinaShortResponseDto
{
    public Guid Id { get; init; }

    public Guid VacinaId { get; init; }

    public VacinaShortResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public string NumeroLote { get; init; } = string.Empty;
}

public sealed record AplicacaoVacinaReadResponseDto
{
    public Guid Id { get; init; }

    public Guid AnimalId { get; init; }

    public Guid VacinaId { get; init; }

    public AnimalShortResponseDto? Animal { get; init; }

    public VacinaShortResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public DateOnly? DataProximaDose { get; init; }

    public bool CicloFinalizado { get; init; } = true;

    public StatusComprovanteVacina StatusComprovante { get; init; } = StatusComprovanteVacina.NaoEmitido;

    public string? ComprovanteDocumentoPath { get; init; }

    public string NumeroLote { get; init; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }
}

public sealed record AplicacaoVacinaDetailResponseDto
{
    public Guid Id { get; init; }

    public Guid AnimalId { get; init; }

    public Guid VacinaId { get; init; }

    public AnimalReadResponseDto? Animal { get; init; }

    public VacinaReadResponseDto? Vacina { get; init; }

    public DateOnly DataAplicacao { get; init; }

    public DateOnly? DataProximaDose { get; init; }

    public bool CicloFinalizado { get; init; } = true;

    public required string NumeroLote { get; init; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; init; }

    public string? Observacoes { get; init; }

    public string? VeterinarioResponsavel { get; init; }

    public string? Aplicador { get; init; }

    public string? LaboratorioFabricante { get; init; }

    public StatusComprovanteVacina StatusComprovante { get; init; } = StatusComprovanteVacina.NaoEmitido;

    public string? ComprovanteDocumentoPath { get; init; }

    public bool TemComprovanteAnexo => !string.IsNullOrWhiteSpace(ComprovanteDocumentoPath);

    public DateTimeOffset CreationDateTime { get; init; }

    public DateTimeOffset? LastModificationDateTime { get; init; }

    public DateTimeOffset CriadoEm => CreationDateTime;

    public DateTimeOffset? AtualizadoEm => LastModificationDateTime;
}

public sealed record ItemAnimalLoteDto
{
    [Required]
    [NonEmptyGuid]
    public Guid AnimalId { get; init; }

    public bool CicloFinalizado { get; init; } = true;
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

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; init; }

    public string? Observacoes { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "A lista de animais deve conter pelo menos um animal.")]
    public required List<ItemAnimalLoteDto> Animais { get; init; }
}

