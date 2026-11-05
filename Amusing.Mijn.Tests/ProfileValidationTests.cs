using System.ComponentModel.DataAnnotations;
using Amusing.Mijn.Services;
using Xunit;
namespace Amusing.Mijn.Tests;
public class ProfileValidationTests
{
    [Theory]
    [InlineData("",true)]
    [InlineData("NL91 ABNA 0417 1643 00",true)]
    [InlineData("NL90ABNA0417164300",false)]
    [InlineData("not-an-iban",false)]
    public void IbanChecksumIsValidated(string iban,bool expected)
    {
        var input=new ChoirProfileInput{Name="Koor",City="Hengelo",Country="NL",GenreId=1,Version="test",BankAccount=iban};
        input.Normalize();
        Assert.Equal(expected,Validator.TryValidateObject(input,new ValidationContext(input),new List<ValidationResult>(),true));
    }
    [Fact] public void EmptyOptionalFieldsAreSafe()
    {
        var input=new ChoirProfileInput{Name="Koor",City="Hengelo",Country="NL",GenreId=1,Version="test",BankAccount=null!,Website=null!};
        Assert.True(Validator.TryValidateObject(input,new ValidationContext(input),new List<ValidationResult>(),true));
    }
}
