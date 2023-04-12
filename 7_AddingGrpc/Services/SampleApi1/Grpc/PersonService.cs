using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SampleApi1.Infrastructure;

namespace GrpcPerson;

public class PersonService : Person.PersonBase
{
    private readonly PersonContext _context;

    public PersonService(PersonContext context)
    {
        this._context = context;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }


    [AllowAnonymous]
    public override async Task<PersonResponse> GetPersonById(PersonRequest request, ServerCallContext context)
    {
        var person = await _context.People.SingleOrDefaultAsync(ci => ci.Id == request.Id);

        if (person == null)
        {
            context.Status = new Status(StatusCode.NotFound, $"Person with id {request.Id} does not exist");
            return default(PersonResponse);
        }

        var personResponse = MapToPersonResponse(person);

        return personResponse;
    }

    public override async Task<PersonResponse> UpdatePerson(PersonRequest request, ServerCallContext context)
    {
        var person = await _context.People.SingleOrDefaultAsync(ci => ci.Id == request.Id);

        if (person == null)
        {
            context.Status = new Status(StatusCode.NotFound, $"Person with id {request.Id} does not exist");
            return default(PersonResponse);
        }

        var personToUpdate = new SampleApi1.Models.Person
        {
            Id = request.Id,
            FirstName = request.Firstname,
            LastName = request.Lastname
        };

        person = personToUpdate;

        _context.People.Update(person);

        await _context.SaveChangesAsync();

        var personResponse = MapToPersonResponse(person);

        return personResponse;

    }

    private static PersonResponse MapToPersonResponse(SampleApi1.Models.Person? person)
    {
        var personResponse = new PersonResponse
        {
            Id = person.Id,
            Firstname = person.FirstName,
            Lastname = person.LastName
        };

        return personResponse;
    }
}