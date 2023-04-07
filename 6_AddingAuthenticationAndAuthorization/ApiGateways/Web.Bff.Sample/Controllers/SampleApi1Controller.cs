using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Web.Bff.Sample.Models;
using System.Net;
using System.Text.Json;

namespace Web.Bff.Sample.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class SampleApi1Controller : ControllerBase
{
    private readonly HttpClient apiClient;
    private readonly UrlsConfig urls;
    public SampleApi1Controller(HttpClient apiClient, IOptions<UrlsConfig> urlsConfig)
    {
        this.urls = urlsConfig.Value;
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

        var url = $"{urls.SampleApi1}{UrlsConfig.PersonOperations.GetPerson(id)}";
        //var content = new StringContent(JsonSerializer.Serialize(id), System.Text.Encoding.UTF8, "application/json");
        var response = await apiClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var personResponse = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PersonData>(personResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
