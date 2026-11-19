using Amusing.Facturatie;
using Amusing.Mijn.Services;

namespace Amusing.Mijn.Tests;

public sealed class PaymentTermTests
{
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
