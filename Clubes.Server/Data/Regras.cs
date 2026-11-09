using Clubes.Shared;
using Microsoft.EntityFrameworkCore;
namespace Clubes.Server.Data;

public static class Regras
{
    // Há outra reserva (pendente ou aprovada) que cruza o período?
    public static Task<bool> TemConflito(ClubesDb db, int clubeId, DateTime ini, DateTime fim, int ignorarId = 0) =>
        db.Reservas.AnyAsync(x => x.ClubeId == clubeId && x.Id != ignorarId
            && x.Status != StatusAprovacao.Rejeitado && ini < x.Fim && x.Inicio < fim);

    // Gera a mensalidade do mês para os associados aprovados (simpatizante não paga). Retorna quantas criou.
    public static async Task<int> GerarMensalidades(ClubesDb db, int? clubeId, DateTime mes)
    {
        var comp = new DateTime(mes.Year, mes.Month, 1);
        var venc = comp.AddDays(9);
        var valores = await db.Clubes.ToDictionaryAsync(c => c.Id, c => c.ValorMensalidade);
        var assoc = await db.Associados
            .Where(a => (clubeId == null || a.ClubeId == clubeId)
                        && a.Aprovacao == StatusAprovacao.Aprovado && a.Tipo != TipoAssociado.Simpatizante)
            .ToListAsync();
        var jaTem = (await db.Mensalidades.Where(m => m.Competencia == comp).Select(m => m.AssociadoId).ToListAsync()).ToHashSet();
        int n = 0;
        foreach (var a in assoc.Where(a => !jaTem.Contains(a.Id)))
        {
            db.Mensalidades.Add(new Mensalidade { AssociadoId = a.Id, Competencia = comp, Vencimento = venc, Valor = valores[a.ClubeId] });
            n++;
        }
        await db.SaveChangesAsync();
        return n;
    }
}
