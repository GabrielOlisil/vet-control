using Api.DTOs.Animals;
using Api.DTOs.Especies;
using Api.DTOs.Racas;
using Api.Models;
using Api.Models.Enums;

namespace Api.Mappers;

public static class AnimalMapper
{
    public static AnimaReadResponseDto MapToHead(Animal animal) => new()
    {
        Id = animal.Id,
        Name = animal.Name,
        Raca = animal.Raca != null ? new RacaShortResponseDto
        {
            Id = animal.Raca.Id,
            Nome = animal.Raca.Nome,
        } : null,
        DataNascimento = animal.DataNascimento,
    };

    public static AnimalShortResponseDto MapToShort(Animal animal) => new()
    {
        Id = animal.Id,
        Name = animal.Name
    };

    public static AnimalDetailResponseDto MapToResponse(Animal animal)
    {
        var identificadores = animal.Identificadores
            .Select(i => new IdentificadorResponseDto(i.Id, i.Tipo, i.Valor, i.IsPrincipal))
            .ToList();

        return new AnimalDetailResponseDto
        {
            Id = animal.Id,
            Name = animal.Name,
            DataNascimento = animal.DataNascimento,
            DataNascimentoAproximada = false,
            Sexo = animal.Sexo,
            Origem = animal.OrigemAnimal,
            LoteOuPasto = animal.LoteOuPasto,
            Raca = animal.Raca is null
                ? null
                : new RacaDetailResponseDto
                {
                    Id = animal.Raca.Id,
                    Nome = animal.Raca.Nome,
                    Especie = animal.Raca.Especie is null
                        ? new EspecieShortResponseDto { Id = animal.Raca.EspecieId, FullName = string.Empty }
                        : new EspecieShortResponseDto
                        {
                            Id = animal.Raca.Especie.Id,
                            FullName = string.Concat(animal.Raca.Especie.Nome, "  |  ", animal.Raca.Especie.NomeCientifico),
                        },
                    AnimalCount = 0
                },
            Identificadores = identificadores,
            IdentificadorPrincipal = identificadores.FirstOrDefault(i => i.EhPrincipal)
                ?? identificadores.FirstOrDefault()
        };
    }
}