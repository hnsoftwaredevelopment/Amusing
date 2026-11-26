using System.Text.Json;
using Amusing.Models;

namespace Amusing.Services;

public sealed record RegistrationInvoiceMetadata(DateOnly IssuedOn,DateOnly? DueOn,decimal? Amount)
{
 public static RegistrationInvoiceMetadata? Read(string json)
 {
  try
  {
   using var doc=JsonDocument.Parse(json);var root=doc.RootElement;
   DateOnly? Date(string key)=>root.TryGetProperty(key,out var value) && value.ValueKind==JsonValueKind.String && DateOnly.TryParseExact(value.GetString(),"yyyy-MM-dd",out var date)?date:null;
   if(root.ValueKind!=JsonValueKind.Object || Date("IssuedOn") is not {} issued)return null;
   decimal? amount=null;
   if(root.TryGetProperty("Amount",out var details) && details.ValueKind==JsonValueKind.Object && details.TryGetProperty("TotaalInclBtw",out var total) && total.ValueKind==JsonValueKind.Number && total.TryGetDecimal(out var number) && number>=0)amount=number;
   return new(issued,Date("PaymentDueOn"),amount);
  }
  catch(JsonException){return null;}
 }
 public void Apply(RegistrationFinancialFields row)
 {
  row.Factuurdatum=IssuedOn.ToDateTime(TimeOnly.MinValue);
  if(DueOn is {} due)row.UitersteBetaaldatum=due.ToDateTime(TimeOnly.MinValue);
  if(Amount is {} amount)row.TeBetalen=amount;
 }
 public static async Task<Dictionary<(uint Festival,uint Group),RegistrationInvoiceMetadata>> LoadAsync(GenericDataService data,uint? festival=null,uint? group=null)
 {
  var rows=await data.ExecuteQueryAsync("""
   SELECT festival_id,zanggroep_id,gegevens,0 AS priority,0 AS id FROM mijn_deelname_notas
   WHERE (@festival IS NULL OR festival_id=@festival) AND (@group IS NULL OR zanggroep_id=@group)
   UNION ALL
   SELECT festival_id,zanggroep_id,nota_gegevens,1 AS priority,id FROM mijn_inschrijfmail
   WHERE soort='nota' AND nota_gegevens IS NOT NULL
    AND (@festival IS NULL OR festival_id=@festival) AND (@group IS NULL OR zanggroep_id=@group)
   """,r=>new{Festival=Convert.ToUInt32(r[0]),Group=Convert.ToUInt32(r[1]),Note=Read(r.GetString(2)),Priority=Convert.ToInt32(r[3]),Id=Convert.ToUInt64(r[4])},
   new(){["@festival"]=(object?)festival??DBNull.Value,["@group"]=(object?)group??DBNull.Value});
  return rows.Where(r=>r.Note is not null).GroupBy(r=>(r.Festival,r.Group))
   .ToDictionary(g=>g.Key,g=>g.OrderByDescending(r=>r.Note!.IssuedOn).ThenByDescending(r=>r.Priority).ThenByDescending(r=>r.Id).First().Note!);
 }
}
