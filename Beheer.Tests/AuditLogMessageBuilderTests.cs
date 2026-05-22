using Amusing.Helpers;
using Amusing.Models;

using Xunit;

namespace Beheer.Tests;

public class AuditLogMessageBuilderTests
{
    [Fact]
    public void BuildChangeReport_DescribesTextValueChangeInDutch()
    {
        var change = new PropertyChange
        {
            PropertyName = "e-mailadres",
            OldValue = "oud@example.nl",
            NewValue = "nieuw@example.nl"
        };

        string report = AuditLogMessageBuilder.BuildChangeReport( "Pauline Huizinga", change );

        Assert.Equal(
            "<_userName> heeft het e-mailadres aangepast van Pauline Huizinga van 'oud@example.nl' naar 'nieuw@example.nl'.",
            report );
    }

    [Fact]
    public void BuildChangeReport_DescribesImageChangeWithoutByteValues()
    {
        var change = new PropertyChange
        {
            PropertyName = "logo",
            OldValue = "System.Byte[]",
            NewValue = "System.Byte[]"
        };

        string report = AuditLogMessageBuilder.BuildChangeReport( "het koor Just4Fun", change );

        Assert.Equal(
            "<_userName> heeft het logo aangepast van het koor Just4Fun.",
            report );
    }

    [Fact]
    public void BuildChangeReport_UsesDutchArticleForPhoto()
    {
        var change = new PropertyChange
        {
            PropertyName = "foto",
            OldValue = "System.Byte[]",
            NewValue = "System.Byte[]"
        };

        string report = AuditLogMessageBuilder.BuildChangeReport( "het koor Just4Fun", change );

        Assert.Equal(
            "<_userName> heeft de foto aangepast van het koor Just4Fun.",
            report );
    }

    [Fact]
    public void BuildPersonName_UsesGridNameWhenSeparateNameFieldsAreMissing()
    {
        var person = new PersonModel
        {
            Name = "Pauline Huizinga",
            FirstName = "",
            NameInfix = "",
            LastName = ""
        };

        string name = AuditLogMessageBuilder.BuildPersonName( person );

        Assert.Equal( "Pauline Huizinga", name );
    }
}
