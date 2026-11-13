using Api.Database;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Ambientes;

public sealed class AmbienteService(ApiContext context)
{
    public IQueryable<Ambiente> Search(string? search)
    {
        var query = context.Ambientes.AsNoTracking();
        if (string.IsNullOrWhiteSpace(search))
            return query;
        var escaped = search.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
        return query.Where(a => EF.Functions.ILike(a.Nome, $"%{escaped}%", @"\"));
    }

    public async Task<Ambiente?> ResolveAsync(Guid? id, string? name, CancellationToken cancellationToken)
    {
        var text = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        if (!id.HasValue && text is null) return null;
        if (id.HasValue && text is not null)
            throw new ArgumentException("Informe apenas o identificador do ambiente ou seu nome cadastrado.");

        var query = context.Ambientes.AsNoTracking();
        var ambiente = id.HasValue
            ? await query.SingleOrDefaultAsync(a => a.Id == id.Value, cancellationToken)
            : await query.SingleOrDefaultAsync(a => a.NomeNormalizado == text!.ToLower(), cancellationToken);
        return ambiente ?? throw new ArgumentException("Selecione um ambiente cadastrado.");
    }
}
