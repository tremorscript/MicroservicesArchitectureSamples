using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SampleWebApp.Models;
using System.Net;
using System.Text.Json;

namespace SampleWebApp.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class PersonController : ControllerBase
{
    private readonly HttpClient apiClient;
    private readonly AppSettings settings;
    public PersonController(HttpClient apiClient, IOptions<AppSettings> settings)
    {
        this.settings = settings.Value;
        this.apiClient = apiClient;
    }

    //Get api/v1/[controller]/:id
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(PersonData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PersonData>> PersonByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var url = $"{settings.BffUrl}/api/v1/SampleApi1/{id}";
        //var content = new StringContent(JsonSerializer.Serialize(id), System.Text.Encoding.UTF8, "application/json");
        var response = await apiClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var personResponse = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PersonData>(personResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    //PUT api/v1/[controller]/update
    [Route("update")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult<PersonData>> UpdatePersonAsync([FromBody] PersonData personToUpdate)
    {
        if (personToUpdate.Id <= 0)
        {
            return BadRequest();
        }

        var url = $"{settings.BffUrl}/s/api/v1/person/update";
        var content = new StringContent(JsonSerializer.Serialize(personToUpdate), System.Text.Encoding.UTF8, "application/json");
        var response = await apiClient.PutAsync(url,content);

        response.EnsureSuccessStatusCode();

        var personResponse = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PersonData>(personResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
 }
