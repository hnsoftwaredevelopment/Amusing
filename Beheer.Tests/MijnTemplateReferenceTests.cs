using Amusing.Services;
using Xunit;

namespace Beheer.Tests;

public sealed class MijnTemplateReferenceTests
{
    [Theory]
    [InlineData("aanmelding")]
    [InlineData("wachtlijst")]
    [InlineData("plaatsing")]
    [InlineData("organisatie")]
    [InlineData("notamail")]
    [InlineData("kopienota")]
    [InlineData("nota_pdf")]
    public void EveryDocumentedVariableHasAnExampleAndExplanation(string kind)
    {
        var definitions=MijnTemplateReference.ForKind(kind);
        Assert.NotEmpty(definitions);
        foreach(var language in new[]{"nl","de","en"})
        foreach(var scenario in new[]{"normaal","contactpersoon1","vlakvoor","wachtlijst","geennota"})
        {
            var examples=MijnTemplateReference.Examples(kind,language,scenario);
            Assert.Equal(definitions.Count,examples.Count);
            Assert.All(definitions,definition =>
            {
                Assert.False(string.IsNullOrWhiteSpace(definition.Explanation));
                Assert.True(examples.ContainsKey(definition.Name));
            });
        }
    }

    [Fact]
    public void NoteUsesItsRecipientAndShowsNoInventedPaymentBalance()
    {
        var treasurer=MijnTemplateReference.Examples("notamail","nl","normaal");
        var primary=MijnTemplateReference.Examples("notamail","nl","contactpersoon1");
        Assert.Equal("Robin Voorbeeld",treasurer["VolledigenaamNotaOntvanger"]);
        Assert.Equal("Anna Voorbeeld",primary["VolledigenaamNotaOntvanger"]);
        Assert.Equal("100,00",primary["TotaalbedragNota"]);
        Assert.Equal("1 april 2027",primary["EindeInschrijvingFestival"]);
        Assert.Equal("26 februari 2027",primary["UitersteBetaaldatum"]);
        Assert.DoesNotContain(primary.Keys,key=>key.Contains("saldo",StringComparison.OrdinalIgnoreCase));
        var late=MijnTemplateReference.Examples("notamail","nl","vlakvoor");
        Assert.Equal("Voldoe de nota voor 1 april 2027.",late["Betaaltermijn"]);
    }

    [Fact]
    public void OrganisationStaysDutchAndWaitlistHasNoInvoiceRecipient()
    {
        var fields=MijnTemplateReference.Examples("organisatie","de","wachtlijst");
        Assert.Equal("",fields["RolNotaOntvanger"]);
        Assert.Equal("Geen nota bij een wachtlijstinschrijving.",fields["MailNotaOntvanger"]);
        Assert.Contains("wachtlijst",fields["Inschrijfstatus"]);
        var confirmation=MijnTemplateReference.Examples("aanmelding","de","contactpersoon1");
        Assert.Equal("Erste Kontaktperson",confirmation["RolNotaOntvanger"]);
        Assert.Contains("contact@example.invalid",confirmation["Notamelding"]);
        Assert.Equal("",MijnTemplateReference.Examples("aanmelding","nl","geennota")["Notamelding"]);
    }

    [Fact]
    public void PreviewEncodesValuesAndWarnsAboutVariablesFromAnotherKind()
    {
        var fields=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){["Groepsnaam"]="<script>alert('test')</script>"};
        Assert.Equal("<b>&lt;script&gt;alert(&#39;test&#39;)&lt;/script&gt;</b>",MijnTemplateReference.Expand("<b>{Groepsnaam}</b>",fields,true));
        var note=MijnTemplateReference.Examples("notamail","nl","normaal");
        Assert.Equal(new[]{"Notamelding"},MijnTemplateReference.Unknown("<style>p{color:red}</style>{Notamelding} {Koornaam}",note));
    }
}
