using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace Amusing.Services;

public sealed record PlanningRegistration
{
 public required uint GroupId {get;init;}
 public required string Name {get;init;}
 public DateTime? RegisteredAt {get;init;}
 public string City {get;init;}="";
 public string Stage {get;init;}="";
 public int Singers {get;init;}
 public string Genre {get;init;}="";
 public decimal? PaidAmount {get;init;}
 public DateTime? PaidOn {get;init;}
 public DateTime? ConfirmedOn {get;init;}
 public bool Battle {get;init;}
 public int Inside {get;init;}=1;
 public int Outside {get;init;}=1;
 public bool Waitlisted {get;init;}
 public bool Cancelled {get;init;}
 public bool IsNew {get;init;}
 public string Contact {get;init;}="";
 public string Email {get;init;}="";
 public string Mobile {get;init;}="";
 public IReadOnlyList<string> Conductors {get;init;}=[];
}
public sealed record PlanningExportData(int Year,IReadOnlyList<PlanningRegistration> Registrations);
public sealed record PlanningExportRow(PlanningRegistration Source,string Attention,string Conductors,bool SharedConductor);
public sealed record PlanningExportSection(string Title,string TableName,IReadOnlyList<PlanningExportRow> Rows);

public static class RegistrationPlanningExport
{
 public static readonly IReadOnlyList<string> Headers=Array.AsReadOnly(new[]{"Aandachtspunt","Datum","Naam","Stad","Podium","Zangers","Genre","Betaald","Bevestigd","AcapellaBattle","Binnen","Buiten","Contactpersoon","Mailadres","Telefoonnr","Dirigent","Eigenlijke naam","Bijzonderheden"});
 private static string Clean(string name)=>Regex.Replace(name.Trim(),@"\s+"," ").Normalize(NormalizationForm.FormC);
 private static IEnumerable<string> Names(PlanningRegistration row)=>row.Conductors.Select(Clean).Where(n=>n.Length>0).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n=>n,StringComparer.OrdinalIgnoreCase);
 public static IReadOnlyList<PlanningExportSection> Prepare(IReadOnlyList<PlanningRegistration> registrations)
 {
  var ordered=registrations.OrderBy(r=>r.RegisteredAt??DateTime.MaxValue).ThenBy(r=>r.GroupId).ToArray();
  if(ordered.Select(r=>r.GroupId).Distinct().Count()!=ordered.Length)throw new InvalidOperationException("Een koor komt meer dan één keer in deze editie voor.");
  var ranks=new Dictionary<string,List<uint>>(StringComparer.OrdinalIgnoreCase);
  foreach(var row in ordered.Where(r=>!r.Cancelled))foreach(var name in Names(row))
  {if(!ranks.TryGetValue(name,out var groups))ranks[name]=groups=[];groups.Add(row.GroupId);}
  PlanningExportRow PrepareRow(PlanningRegistration row)
  {
   var shared=false;var names=new List<string>();
   foreach(var name in Names(row))
   {
    if(!row.Cancelled && ranks.TryGetValue(name,out var groups) && groups.Count>1)
    {shared=true;names.Add($"{name} ({groups.IndexOf(row.GroupId)+1}v{groups.Count})");}
    else names.Add(name);
   }
   var attention=row.IsNew?"Nieuw":"";
   if(row.Waitlisted && !row.Cancelled)
   {
    var preference=(row.Inside,row.Outside) switch{(2,0)=>"2 binnen",(0,2)=>"2 buiten",_=>$"{row.Inside} binnen / {row.Outside} buiten"};
    attention=attention.Length==0?preference:attention+" · "+preference;
   }
   return new(row,attention,string.Join('\n',names),shared);
  }
  return [new("Deelnemende koren","PlanningDeelnemers",ordered.Where(r=>!r.Cancelled && !r.Waitlisted).Select(PrepareRow).ToArray()),
   new("Wachtlijst","PlanningWachtlijst",ordered.Where(r=>!r.Cancelled && r.Waitlisted).Select(PrepareRow).ToArray()),
   new("Afgehaakte koren","PlanningAfgehaakt",ordered.Where(r=>r.Cancelled).Select(PrepareRow).ToArray())];
 }
 public static byte[] CreateWorkbook(PlanningExportData data)
 {
  using var book=new XLWorkbook();var sheet=book.Worksheets.Add("Inschrijvingen");
  sheet.Style.Font.FontName="Calibri";sheet.Style.Font.FontSize=11;
  sheet.Cell(1,1).Value=$"INSCHRIJVINGEN {data.Year}";sheet.Cell(1,1).Style.Font.Bold=true;sheet.Cell(1,1).Style.Font.FontSize=16;
  double[] widths=[26,14,36,24,10,10,24,23,16,18,10,10,30,38,19,38,36,60];
  for(var c=0;c<widths.Length;c++)sheet.Column(c+1).Width=widths[c];
  sheet.SheetView.FreezeRows(4);sheet.SheetView.FreezeColumns(3);
  var titleRow=3;
  foreach(var section in Prepare(data.Registrations))
  {
   sheet.Cell(titleRow,1).Value=$"{section.Title} ({section.Rows.Count})";sheet.Cell(titleRow,1).Style.Font.Bold=true;
   var header=titleRow+1;for(var c=0;c<Headers.Count;c++)sheet.Cell(header,c+1).Value=Headers[c];
   var end=header+Math.Max(1,section.Rows.Count);
   var table=sheet.Range(header,1,end,Headers.Count).CreateTable(section.TableName);table.Theme=XLTableTheme.TableStyleMedium2;table.ShowRowStripes=false;table.ShowAutoFilter=true;
   sheet.Row(header).Height=32;sheet.Range(header,1,header,Headers.Count).Style.Alignment.WrapText=true;
   var index=header+1;
   foreach(var row in section.Rows)
   {
    var r=row.Source;var cells=sheet.Range(index,1,index,Headers.Count);
    cells.Style.Alignment.Vertical=XLAlignmentVerticalValues.Top;cells.Style.Alignment.WrapText=true;
    sheet.Cell(index,1).Value=row.Attention;
    if(r.RegisteredAt is {} date){sheet.Cell(index,2).Value=date;sheet.Cell(index,2).Style.DateFormat.Format="dd-mm-yyyy";}
    sheet.Cell(index,3).Value=r.Name;sheet.Cell(index,4).Value=r.City;sheet.Cell(index,5).Value=r.Stage;sheet.Cell(index,6).Value=r.Singers;sheet.Cell(index,7).Value=r.Genre;
    if(r.PaidAmount is {} paid){sheet.Cell(index,8).Value=(double)paid;sheet.Cell(index,8).Style.NumberFormat.Format="#,##0.00";}
    else sheet.Cell(index,8).Value=r.PaidOn is null?"Nee":"Ja (bedrag onbekend)";
    if(r.ConfirmedOn is {} confirmed){sheet.Cell(index,9).Value=confirmed;sheet.Cell(index,9).Style.DateFormat.Format="dd-mm-yyyy";}else sheet.Cell(index,9).Value="Nee";
    sheet.Cell(index,10).Value=r.Battle?"Ja":"Nee";sheet.Cell(index,11).Value=r.Inside;sheet.Cell(index,12).Value=r.Outside;
    sheet.Cell(index,13).Value=r.Contact;sheet.Cell(index,14).Value=r.Email;sheet.Cell(index,15).Value=r.Mobile;sheet.Cell(index,15).Style.NumberFormat.Format="@";
    sheet.Cell(index,16).Value=row.Conductors;sheet.Cell(index,17).Value=r.Name;
    if(r.Waitlisted && !r.Cancelled)sheet.Cell(index,1).Style.Fill.BackgroundColor=XLColor.FromHtml("#FCE4D6");
    else if(r.IsNew)sheet.Cell(index,1).Style.Fill.BackgroundColor=XLColor.FromHtml("#D9EAF7");
    if(row.SharedConductor)sheet.Cell(index,16).Style.Fill.BackgroundColor=XLColor.FromHtml("#FFF2CC");
    var lines=Math.Max(row.Conductors.Count(c=>c=='\n')+1,Math.Max((r.Name.Length+34)/35,(r.Contact.Length+29)/30));sheet.Row(index).Height=Math.Max(36,lines*15+6);
    index++;
   }
   titleRow=end+3;
  }
  using var output=new MemoryStream();book.SaveAs(output);return output.ToArray();
 }
}
