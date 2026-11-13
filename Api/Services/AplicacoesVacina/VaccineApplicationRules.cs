using System.Globalization;
using Api.Models;

namespace Api.Services.AplicacoesVacina;

public static class VaccineApplicationRules
{
    public static void Validate(AplicacaoVacina record, Animal animal, Vacina vaccine, DateOnly today, bool historicalCorrection = false)
    {
        if (!historicalCorrection && (!animal.Ativo || !vaccine.Ativo))
            throw new ArgumentException("Novas aplicações exigem animal e vacina ativos.");
        if (vaccine.EspecieId.HasValue && animal.Raca?.EspecieId != vaccine.EspecieId)
            throw new ArgumentException("A vacina não é compatível com a espécie do animal.");
        if (record.DataAplicacao == default || record.DataAplicacao > today)
            throw new ArgumentException("A aplicação deve ter uma data válida, não futura.");
        if (record.DataAplicacao < animal.DataNascimento)
            throw new ArgumentException("A aplicação não pode ser anterior ao nascimento do animal.");
        if (record.DataFabricacao > record.DataAplicacao)
            throw new ArgumentException("A fabricação não pode ser posterior à aplicação.");
        if (record.DataValidade < record.DataAplicacao ||
            record.DataFabricacao.HasValue && record.DataValidade < record.DataFabricacao)
            throw new ArgumentException("A vacina deve estar dentro da validade na data da aplicação.");
        if (record.DataProximaDose <= record.DataAplicacao)
            throw new ArgumentException("A próxima dose deve ser posterior à aplicação.");
        record.NumeroLote = record.NumeroLote?.Trim() ?? "";
        if (record.NumeroLote.Length is < 1 or > 100)
            throw new ArgumentException("Informe um lote de 1 a 100 caracteres.");
        if (record.DoseMl is not null)
        {
            if (!decimal.TryParse(record.DoseMl.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var dose) || dose <= 0)
                throw new ArgumentException("A dose em mL deve ser um número positivo.");
            record.DoseMl = dose.ToString("0.############################", CultureInfo.InvariantCulture);
        }
    }
}
