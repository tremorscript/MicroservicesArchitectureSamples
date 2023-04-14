using GrpcPerson;
using Web.Bff.Sample.Models;

namespace Web.Bff.Sample.Services;

public class PersonService : IPersonService
{
    private readonly Person.PersonClient _personClient;
    private readonly ILogger<PersonService> _logger;

    public PersonService(Person.PersonClient personClient, ILogger<PersonService> logger)
    {
        this._personClient = personClient;
        this._logger = logger;
    }

    public async Task<PersonData> GetPersonByIdAsync(int id)
    {
        _logger.LogDebug("grpc client created, request = {@id}", id);
        var response = await _personClient.GetPersonByIdAsync(new PersonRequest { Id = id });
        _logger.LogDebug("grpc response {@response}", response);
        return MapToPersonData(response);
    }

    private static PersonData MapToPersonData(PersonResponse response)
    {
        if (response == null)
        {
            return null;
        }

        return new PersonData
        {
            Id = response.Id,
            FirstName = response.Firstname,
            LastName = response.Lastname
        };
    }

    public async Task<PersonData> UpdatePersonAsync(PersonData currentPerson)
    {
        _logger.LogDebug("Grpc update person currentPerson {@currentPerson}", currentPerson);
        var personRequest = new PersonRequest
        {
            Id = currentPerson.Id,
            Firstname = currentPerson.FirstName,
            Lastname = currentPerson.LastName
        };
        var response = await _personClient.UpdatePersonAsync(personRequest);
        _logger.LogDebug("Grpc update person response {@response}", response);

        return MapToPersonData(response);
    }
}