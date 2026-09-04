using Api.DTOs.Animals;
using Api.Models;

namespace Api.Mappers;

public static class AnimalMapper
{
    public static AnimalReadDto MapToHead(Animal animal) => new()
    {
        Id = animal.Id,
        Name = animal.Name,
        RacaId = animal.RacaId,
        CartaoVacinaId = animal.CartaoVacinaId,
        DataNascimento = animal.DataNascimento,
        PictureUpload = animal.PictureUpload
    };

    public static AnimalResponseDto MapToResponse(Animal animal) => new()
    {
        Id = animal.Id,
        Nome = animal.Name,
        DataNascimento = animal.DataNascimento,
        PictureUpload = animal.PictureUpload,
        Raca = animal.Raca is null
            ? null
            : new RacaInfoDto
            {
                Id = animal.Raca.Id,
                Nome = animal.Raca.Nome,
                EspecieNome = animal.Raca.Especie?.Nome ?? "N/A"
            },
        CartaoVacina = animal.CartaoVacina is null
            ? null
            : new CartaoVacinaInfoDto
            {
                Id = animal.CartaoVacina.Id,
                VacinasAplicadas = animal.CartaoVacina.VacinasAplicadas.Select(va => new AplicacaoVacinaInfoDto
                {
                    Id = va.Id,
                    VacinaName = va.Vacina?.Name ?? "N/A",
                    ReaplicarEmXDias = va.Vacina?.ReaplicarEmXDias ?? 0,
                    DataAplicacao = va.DataAplicacao
                }).ToList()
            }
    };
}