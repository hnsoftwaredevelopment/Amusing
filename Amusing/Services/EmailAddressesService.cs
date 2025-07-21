using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class EmailAddressesService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<EmailAddressesModel>> GetNewsletterEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetNewsletterEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }

    public Task<List<EmailAddressesModel>> GetAllKnownEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetAllKnownEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }
    public Task<List<EmailAddressesModel>> GetNewlyAddedEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetNewlyAddedEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }

    public Task<List<EmailAddressesModel>> GetOldEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetOldEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }

    public Task<List<EmailAddressesModel>> GetPreviousEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPreviousEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }

    public Task<List<EmailAddressesModel>> GetUpcommingEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetUpcommingEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }

    public Task<List<EmailAddressesModel>> GetQueueUpcommingEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetQueueUpcommingEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }


    public Task<List<EmailAddressesModel>> GetIncompleteEmailAddressesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetIncompleteEmailAddresses,
            reader => new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Ontbreekt = reader [ "Ontbreekt" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            }
        );
    }
}
