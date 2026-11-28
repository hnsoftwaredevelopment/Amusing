using Amusing.Services;
using ClosedXML.Excel;
using Xunit;
namespace Beheer.Tests;
public class RegistrationPlanningExportTests
{
 private static PlanningRegistration Row(uint id,string name,params string[] conductors)=>new(){GroupId=id,Name=name,RegisteredAt=new DateTime(2026,11,1),Conductors=conductors};
 [Fact]public void CategoriesAreExclusiveAndConductorsCountDistinctNonCancelledChoirs()
 {
  var rows=new[]{Row(2,"Second"," Jan  de Vries ","Solo"),Row(1,"First","JAN DE VRIES","Jan de Vries") with{IsNew=true},Row(3,"Waiting","Jan de Vries") with{Waitlisted=true,IsNew=true,Inside=0,Outside=2},Row(4,"Cancelled","Jan de Vries") with{Cancelled=true,Waitlisted=true}};
  var sections=RegistrationPlanningExport.Prepare(rows);
  Assert.Equal(3,sections.Count);Assert.Equal(new uint[]{1,2},sections[0].Rows.Select(r=>r.Source.GroupId));Assert.Single(sections[1].Rows);Assert.Single(sections[2].Rows);
  Assert.Equal("JAN DE VRIES (1v3)",sections[0].Rows[0].Conductors);
  Assert.Equal("Jan de Vries (2v3)\nSolo",sections[0].Rows[1].Conductors);
  Assert.Equal("Nieuw · 2 buiten",sections[1].Rows[0].Attention);Assert.True(sections[1].Rows[0].SharedConductor);
  Assert.Equal("Jan de Vries",sections[2].Rows[0].Conductors);Assert.False(sections[2].Rows[0].SharedConductor);
 }
 [Fact]public void NameMatchingKeepsAccentsAndPunctuationAndSortsEqualDatesByChoirId()
 {
  var sections=RegistrationPlanningExport.Prepare(new[]{Row(3,"C","José"),Row(2,"B"," Jose "),Row(1,"A","JOSE"),Row(4,"D","Jo-se")});
  Assert.Equal("JOSE (1v2)",sections[0].Rows[0].Conductors);Assert.Equal("Jose (2v2)",sections[0].Rows[1].Conductors);Assert.Equal("José",sections[0].Rows[2].Conductors);Assert.Equal("Jo-se",sections[0].Rows[3].Conductors);
 }
 [Fact]public void WorkbookHasThreeNativeTablesWithTypedDatesAndEditablePlanningColumns()
 {
  var rows=new[]{Row(1,"=1+1","Same") with{IsNew=true,Mobile="0612345678"},Row(2,"Two","Same") with{Waitlisted=true,Inside=2,Outside=0},Row(3,"Three") with{Cancelled=true}};
  var bytes=RegistrationPlanningExport.CreateWorkbook(new(2027,rows));using var book=new XLWorkbook(new MemoryStream(bytes));var sheet=Assert.Single(book.Worksheets);
  Assert.Equal(3,sheet.Tables.Count());
  foreach(var table in sheet.Tables){Assert.True(table.ShowAutoFilter);Assert.Equal(RegistrationPlanningExport.Headers,table.Fields.Select(f=>f.Name));}
  var table1=sheet.Tables.Single(t=>t.Name=="PlanningDeelnemers");var row=table1.DataRange!.FirstRow();
  Assert.Equal(XLDataType.DateTime,row.Cell(2).DataType);Assert.Equal("=1+1",row.Cell(3).GetString());Assert.False(row.Cell(3).HasFormula);
  Assert.Equal(row.Cell(3).GetString(),row.Cell(17).GetString());Assert.True(row.Cell(18).IsEmpty());Assert.Equal("Nee",row.Cell(8).GetString());
  Assert.Equal(XLColor.FromHtml("#FFF2CC").Color,row.Cell(16).Style.Fill.BackgroundColor.Color);
  Assert.Equal("0612345678",row.Cell(15).GetString());Assert.Equal(XLDataType.Text,row.Cell(15).DataType);
  Assert.Equal(XLColor.FromHtml("#D9EAF7").Color,row.Cell(1).Style.Fill.BackgroundColor.Color);
  Assert.Equal(XLColor.FromHtml("#FCE4D6").Color,sheet.Tables.Single(t=>t.Name=="PlanningWachtlijst").DataRange!.FirstRow().Cell(1).Style.Fill.BackgroundColor.Color);
 }
 [Fact]public void EmptyCategoriesStillHaveTheirOwnFilterableTableWithoutInventedChoirs()
 {
  using var book=new XLWorkbook(new MemoryStream(RegistrationPlanningExport.CreateWorkbook(new(2027,[]))));
  Assert.Equal(3,book.Worksheet(1).Tables.Count());
  foreach(var table in book.Worksheet(1).Tables)Assert.All(table.DataRange!.Column(3).Cells(),cell=>Assert.True(cell.IsEmpty()));
 }
}
