using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.DTOs.Animals;
using Api.DTOs.Vacinas;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs.AplicacoesVacina;

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

public sealed class AplicacaoVacinaCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid AnimalId { get; set; }

    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; set; }

    [Required]
    public required string NumeroLote { get; set; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; set; }

    public string? Observacoes { get; set; }

    public string? VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? LaboratorioFabricante { get; set; }
}

public sealed class AplicacaoVacinaPatchDto
{
    public Guid? AnimalId { get; set; }

    public Guid? VacinaId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataAplicacao { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; set; }

    public string? NumeroLote { get; set; }

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? DoseMl { get; set; }

    public string? Observacoes { get; set; }

    public string? VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? LaboratorioFabricante { get; set; }
}

public sealed class AplicacaoVacinaShortResponseDto
{
    public Guid Id { get; set; }

    public Guid VacinaId { get; set; }

    public VacinaShortResponseDto? Vacina { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public string NumeroLote { get; set; } = string.Empty;
}

public sealed class AplicacaoVacinaReadResponseDto
{
    public Guid Id { get; set; }

    public Guid AnimalId { get; set; }

    public Guid VacinaId { get; set; }

    public AnimalShortResponseDto? Animal { get; set; }

    public VacinaShortResponseDto? Vacina { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public DateOnly DataProximaDose { get; set; }

    public string NumeroLote { get; set; } = string.Empty;

    public string? DoseMl { get; set; }
}

public sealed class AplicacaoVacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public Guid AnimalId { get; set; }

    public Guid VacinaId { get; set; }

    public AnimalReadResponseDto? Animal { get; set; }

    public VacinaReadResponseDto? Vacina { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public DateOnly DataProximaDose { get; set; }

    public required string NumeroLote { get; set; }

    public string? DoseMl { get; set; }

    public string? Observacoes { get; set; }

    public string? VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? LaboratorioFabricante { get; set; }

    public StatusComprovanteVacina StatusComprovante { get; set; } = StatusComprovanteVacina.NaoEmitido;

    public bool TemComprovanteAnexo { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }

    public DateTimeOffset? LastModificationDateTime { get; set; }

    public DateTimeOffset CriadoEm => CreationDateTime;

    public DateTimeOffset? AtualizadoEm => LastModificationDateTime;
}

