using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleApi1.Infrastructure;
using SampleApi1.Models;
using System.Net;

namespace SampleApi1.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class PersonController : ControllerBase
{
    private readonly PersonContext _personContext;

    public PersonController(PersonContext context)
    {
        _personContext = context;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    //Get api/v1/[controller]/:id
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Person), (int)HttpStatusCode.OK)]
    [Authorize]
    public async Task<ActionResult<Person>> PersonByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = await _personContext.People.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item != null)
        {
            return item;
        }

        return NotFound();
    }

    //PUT api/v1/[controller]/update
    [Route("update")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdatePersonAsync([FromBody] Person personToUpdate)
    {
        var person = await _personContext.People.SingleOrDefaultAsync(i => i.Id == personToUpdate.Id);

        if (person == null)
        {
            return NotFound(new { Message = $"Item with id {personToUpdate.Id} not found." });
        }

        // Update current person
        person = personToUpdate;
        _personContext.People.Update(person);

        await _personContext.SaveChangesAsync();

        return CreatedAtAction(nameof(PersonByIdAsync), new { id = personToUpdate.Id }, personToUpdate);
    }

    //DELETE api/v1/[controller]/id
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeletePersonAsync(int id)
    {
        var person = await _personContext.People.SingleOrDefaultAsync(x => x.Id == id);

        if (person == null)
        {
            return NotFound();
        }

        _personContext.People.Remove(person);

        await _personContext.SaveChangesAsync();

        return NoContent();
    }

    //POST api/v1/[controller]/create
    [Route("create")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreatePersonAsync([FromBody] Person person)
    {
        var item = new Person
        {
            FirstName = person.FirstName,
            LastName = person.LastName,
        };

        _personContext.People.Add(item);

        await _personContext.SaveChangesAsync();

        return CreatedAtAction(nameof(PersonByIdAsync), new { id = item.Id }, null);
    }
}