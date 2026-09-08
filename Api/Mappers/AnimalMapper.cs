using Api.DTOs.Animals;
using Api.DTOs.CartoesVacina;
using Api.DTOs.Racas;
using Api.Models;

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

    public static AnimalDetailResponseDto MapToResponse(Animal animal) => new()
    {
        Id = animal.Id,
        Nome = animal.Name,
        DataNascimento = animal.DataNascimento,
        PictureUpload = animal.PictureUpload,
        Raca = animal.Raca is null
            ? null
            : new RacaShortResponseDto
            {
                Id = animal.Raca.Id,
                Nome = animal.Raca.Nome,
            },
        CartaoVacina = animal.CartaoVacina is null
            ? null
            : new CartaoVacinaShortResponseDto
            {
                Id = animal.CartaoVacina.Id,
                NumVacinas = (uint)animal.CartaoVacina.VacinasAplicadas.Count
            }
    };
}