using Amusing.Components.Pages;
using Amusing.Models;

using Xunit;

namespace Beheer.Tests;

public class OverviewEmailAddressesTests
{
    [Fact]
    public void FilterByCountry_ReturnsDutchAddressesWhenLandHasDifferentCasingOrWhitespace()
    {
        var addresses = new List<EmailAddressesModel>
        {
            new() { Email = "nl@example.nl", Land = " NL " },
            new() { Email = "de@example.de", Land = "de" },
            new() { Email = "other@example.com", Land = "be" },
        };

        var result = OverviewEmailAddresses.FilterByCountry(addresses, "nl");

        Assert.Single(result);
        Assert.Equal("nl@example.nl", result[0].Email);
    }

    [Fact]
    public void FilterByCountry_ReturnsDutchAddressesWhenSelectedCountryIsDisplayName()
    {
        var addresses = new List<EmailAddressesModel>
        {
            new() { Email = "nl@example.nl", Land = "NL" },
            new() { Email = "de@example.de", Land = "DE" },
        };

        var result = OverviewEmailAddresses.FilterByCountry(addresses, "Nederland");

        Assert.Single(result);
        Assert.Equal("nl@example.nl", result[0].Email);
    }

    [Fact]
    public void FilterByCountry_ReturnsNonDutchAndNonGermanAddressesForOtherCountries()
    {
        var addresses = new List<EmailAddressesModel>
        {
            new() { Email = "nl@example.nl", Land = "nl" },
            new() { Email = "de@example.de", Land = "de" },
            new() { Email = "be@example.be", Land = "be" },
            new() { Email = "empty@example.com", Land = "" },
        };

        var result = OverviewEmailAddresses.FilterByCountry(addresses, "uk");

        Assert.Equal(["be@example.be", "empty@example.com"], result.Select(address => address.Email));
    }

    [Fact]
    public void ApplyCountryFilters_FiltersUpcomingEditionAddresses()
    {
        var page = new OverviewEmailAddressesHarness();

        page.SetSelectedCountry("nl");
        page.SetUpcomingAddresses(
        [
            new() { Email = "nl@example.nl", Land = "nl" },
            new() { Email = "de@example.de", Land = "de" },
        ]);

        page.ApplyFilters();

        Assert.Equal(1, page.UpcomingCount);
        Assert.Equal("nl@example.nl", page.UpcomingAddresses.Single().Email);
    }

    private sealed class OverviewEmailAddressesHarness : OverviewEmailAddresses
    {
        public int UpcomingCount => UpcommingEmailAddressesListVisibleRowCount;

        public List<EmailAddressesModel> UpcomingAddresses => FilteredUpcommingEmailAddressesList;

        public void SetSelectedCountry(string selectedCountry) => SelectedCountry = selectedCountry;

        public void SetUpcomingAddresses(List<EmailAddressesModel> addresses) => AllUpcommingEmailAddressesList = addresses;

        public void ApplyFilters() => ApplyCountryFilters();
    }
}
