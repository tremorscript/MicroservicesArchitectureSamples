using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Web.Bff.Sample.Models;
using Web.Bff.Sample.Services;

namespace Web.Bff.Sample.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SampleApi1GrpcController : ControllerBase
{
    private readonly IPersonService _personService;

    public SampleApi1GrpcController(IPersonService personService)
    {
        this._personService = personService;
    }

    //Get api/v1/[controller]/:id
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(PersonData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PersonData>> GetPersonByIdAsync(int id)
    {
       var personResponse = await _personService.GetPersonByIdAsync(id); 
       return personResponse;
    }

    //PUT api/v1/[controller]/update
    [Route("update")]
    [HttpPut()]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult<PersonData>> UpdatePersonAsync(PersonData personToUpdate)
    {
        var personResponse = await _personService.UpdatePersonAsync(personToUpdate); 
        return personResponse;
    }
}