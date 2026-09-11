using Api.DTOs;
using Api.Models;

namespace Api.Mappers;

public static class AnimalMapper
{
    public static AnimalReadResponseDto MapToHead(Animal animal)
    {
        var principal = animal.Identificadores.FirstOrDefault(i => i.IsPrincipal)
            ?? animal.Identificadores.FirstOrDefault();

        return new AnimalReadResponseDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Sexo = animal.Sexo,
            Origem = animal.OrigemAnimal,
            DataNascimento = animal.DataNascimento,
            LoteOuPasto = animal.LoteOuPasto,
            Ativo = animal.Ativo,
            Raca = animal.Raca != null ? RacaMapper.MapToShort(animal.Raca) : null,
            IdentificadorPrincipal = principal != null
                ? new IdentificadorResponseDto(principal.Id, principal.Tipo, principal.Valor, principal.IsPrincipal)
                : null
        };
    }

    public static AnimalShortResponseDto MapToShort(Animal animal) => new()
    {
        Id = animal.Id,
        Name = animal.Name,
        IdentificadorPrincipal = (animal.Identificadores.FirstOrDefault(i => i.IsPrincipal)
            ?? animal.Identificadores.FirstOrDefault())?.Valor
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
            Ativo = animal.Ativo,
            Raca = animal.Raca is null ? null : RacaMapper.MapToRead(animal.Raca),
            Identificadores = identificadores,
            IdentificadorPrincipal = identificadores.FirstOrDefault(i => i.EhPrincipal)
                ?? identificadores.FirstOrDefault(),
            CreationDateTime = animal.CreationDateTime,
            LastModificationDateTime = animal.LastModificationDateTime
        };
    }
}