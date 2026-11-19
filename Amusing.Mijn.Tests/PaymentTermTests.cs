using Amusing.Facturatie;
using Amusing.Mijn.Services;

namespace Amusing.Mijn.Tests;

public sealed class PaymentTermTests
{
    [Theory]
    [InlineData("nl", "Voldoe de nota binnen 6 weken; voor u is dit uiterlijk op 31 december 2026.")]
    [InlineData("de", "Bitte begleichen Sie die Rechnung innerhalb von 6 Wochen, spätestens jedoch bis zum 31 Dezember 2026.")]
    [InlineData("en", "Please pay the invoice within 6 weeks; your payment deadline is 31 December 2026.")]
    public void SixWeekPaymentTextIncludesTheConcreteDeadline(string language,string expected) =>
        Assert.Equal(expected,NotaBetaaltermijn.Tekst(new(2026,11,19),new(2027,4,1),new(2026,11,19),language));

    [Theory]
    [InlineData(2027, 1, 1, 2027, 2, 12)]
    [InlineData(2027, 3, 31, 2027, 4, 1)]
    [InlineData(2027, 4, 1, 2027, 4, 1)]
    public void DueDateUsesEarliestOfSixWeeksAndClosingDate(int year,int month,int day,int dueYear,int dueMonth,int dueDay)
    {
        var registered = new DateOnly(year,month,day);
        var note = new ParticipationNote(23,1,new(2027,6,5),registered,"Koor",25,"B",new Deelnamebedrag(75,25,9),false)
        { RegisteredOn = registered, RegistrationClosesOn = new(2027,4,1) };
        Assert.Equal(new DateOnly(dueYear,dueMonth,dueDay),note.PaymentDueOn);
        Assert.Equal(note.PaymentDueOn,(note with { IssuedOn = registered.AddDays(5) }).PaymentDueOn);
        if(month == 3) Assert.Equal("Voldoe de nota voor 1 april 2027.",note.PaymentTerm("nl"));
    }

    [Fact]
    public void LocalizedUiUsesResourceFilesAndDutchFallback()
    {
        Assert.Equal("Gekoppelde personen",UiText.Get("GekoppeldePersonen","nl"));
        Assert.Equal("Linked people",UiText.Get("GekoppeldePersonen","en"));
        Assert.Equal("Verknüpfte Personen",UiText.Get("GekoppeldePersonen","de"));
        Assert.Equal("Register for Amusing 2027",UiText.Format("InschrijvenVoorEditie","en",2027));
    }
}
