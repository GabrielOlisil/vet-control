using Api.Database;
using Api.DTOs;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.AgendaVacinal;

public sealed class AgendaVacinalService(ApiContext db, ClinicalCalendar calendar)
{
    public Task<List<AgendaVacinalItemDto>> GetAsync(int? page, AgendaVacinalSearchDto search, CancellationToken ct = default) =>
        Filter(search).OrderBy(x => x.DataPrevista).ThenBy(x => x.AnimalId)
            .ThenBy(x => x.VacinaId).ThenBy(x => x.ProtocoloVacinalDoseId)
            .Skip(Pagination.Offset(page, 10)).Take(10).ToListAsync(ct);

    public Task<int> CountAsync(AgendaVacinalSearchDto search, CancellationToken ct = default) => Filter(search).CountAsync(ct);

    private IQueryable<AgendaVacinalItemDto> Filter(AgendaVacinalSearchDto search)
    {
        var today = calendar.Today;
        var protocols =
            from assignment in db.AnimalProtocolosVacinais.AsNoTracking()
            where assignment.Ativo && assignment.Animal!.Ativo
            from dose in assignment.ProtocoloVacinal!.Doses
            let last = db.AplicacoesVacina.Where(a => a.AnimalProtocoloVacinalId == assignment.Id &&
                a.ProtocoloVacinalDoseId == dose.Id && a.SituacaoRegistro == SituacaoRegistroVacina.Ativo)
                .OrderByDescending(a => a.DataAplicacao).ThenByDescending(a => a.CreationDateTime).FirstOrDefault()
            let complete = last != null && dose.IntervaloReforcoDias == null
            let due = last != null && dose.IntervaloReforcoDias != null
                ? last.DataAplicacao.AddDays(dose.IntervaloReforcoDias.Value)
                : assignment.DataInicio.AddDays(dose.DiasAposInicio)
            select new AgendaVacinalItemDto
            {
                Origem = OrigemPendenciaVacinal.Protocolo, AnimalId = assignment.AnimalId,
                AnimalNome = assignment.Animal!.Name,
                IdentificadorPrincipal = assignment.Animal!.Identificadores.OrderByDescending(i => i.IsPrincipal).ThenBy(i => i.Id).Select(i => i.Valor).FirstOrDefault(),
                LoteOuPasto = assignment.Animal!.LoteOuPasto,
                AmbienteId = assignment.Animal!.AmbienteId,
                EspecieId = assignment.Animal!.Raca == null ? null : assignment.Animal!.Raca!.EspecieId,
                VacinaId = dose.VacinaId, VacinaNome = dose.Vacina!.Name,
                ProtocoloVacinalId = assignment.ProtocoloVacinalId, ProtocoloNome = assignment.ProtocoloVacinal!.Nome,
                ProtocoloVersao = assignment.ProtocoloVacinal!.Versao,
                AnimalProtocoloVacinalId = assignment.Id, ProtocoloVacinalDoseId = dose.Id,
                AplicacaoVacinaId = last == null ? null : last.Id, DataPrevista = due,
                DataAplicacao = last == null ? null : last.DataAplicacao, Ordem = dose.Ordem, Obrigatoria = dose.Obrigatoria,
                Status = complete ? StatusPendenciaVacinal.Concluida : due < today ? StatusPendenciaVacinal.Atrasada : StatusPendenciaVacinal.Pendente
            };

        var singles = db.AplicacoesVacina.AsNoTracking().Where(a => a.SituacaoRegistro == SituacaoRegistroVacina.Ativo &&
            a.Animal!.Ativo && a.AnimalProtocoloVacinalId == null && a.DataProximaDose != null &&
            !db.AplicacoesVacina.Any(next => next.AnimalId == a.AnimalId && next.VacinaId == a.VacinaId &&
                next.SituacaoRegistro == SituacaoRegistroVacina.Ativo &&
                (next.DataAplicacao > a.DataAplicacao ||
                 next.DataAplicacao == a.DataAplicacao && next.CreationDateTime > a.CreationDateTime)))
            .Select(a => new AgendaVacinalItemDto
            {
                Origem = OrigemPendenciaVacinal.Avulsa, AnimalId = a.AnimalId, AnimalNome = a.Animal!.Name,
                IdentificadorPrincipal = a.Animal.Identificadores.OrderByDescending(i => i.IsPrincipal).ThenBy(i => i.Id).Select(i => i.Valor).FirstOrDefault(),
                LoteOuPasto = a.Animal.LoteOuPasto, EspecieId = a.Animal.Raca == null ? null : a.Animal.Raca.EspecieId,
                AmbienteId = a.Animal.AmbienteId,
                VacinaId = a.VacinaId, VacinaNome = a.Vacina!.Name,
                ProtocoloVacinalId = null, ProtocoloNome = null, ProtocoloVersao = null,
                AnimalProtocoloVacinalId = null, ProtocoloVacinalDoseId = null,
                AplicacaoVacinaId = a.Id, DataPrevista = a.DataProximaDose!.Value, DataAplicacao = a.DataAplicacao,
                Ordem = null, Obrigatoria = true,
                Status = a.DataProximaDose < today ? StatusPendenciaVacinal.Atrasada : StatusPendenciaVacinal.Pendente
            });

        var query = search.Origem switch
        {
            OrigemPendenciaVacinal.Protocolo => protocols,
            OrigemPendenciaVacinal.Avulsa => singles,
            _ => protocols.Concat(singles)
        };
        if (!search.IncluirConcluidas && search.Status != StatusPendenciaVacinal.Concluida)
            query = query.Where(x => x.Status != StatusPendenciaVacinal.Concluida);
        if (search.Status.HasValue) query = query.Where(x => x.Status == search.Status);
        if (search.AnimalId.HasValue) query = query.Where(x => x.AnimalId == search.AnimalId);
        if (search.VacinaId.HasValue) query = query.Where(x => x.VacinaId == search.VacinaId);
        if (search.EspecieId.HasValue) query = query.Where(x => x.EspecieId == search.EspecieId);
        if (search.ProtocoloVacinalId.HasValue) query = query.Where(x => x.ProtocoloVacinalId == search.ProtocoloVacinalId);
        if (!string.IsNullOrWhiteSpace(search.LoteOuPasto)) query = query.Where(x => x.LoteOuPasto == search.LoteOuPasto);
        if (search.AmbienteId.HasValue) query = query.Where(x => x.AmbienteId == search.AmbienteId);
        if (search.SomenteObrigatorias) query = query.Where(x => x.Obrigatoria);
        if (search.DataPrevistaFrom.HasValue) query = query.Where(x => x.DataPrevista >= search.DataPrevistaFrom);
        if (search.DataPrevistaTo.HasValue) query = query.Where(x => x.DataPrevista <= search.DataPrevistaTo);
        return query;
    }
}
