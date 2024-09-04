using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WebApplication1;
using System.Text.Json;

namespace WebApplication1;

[ApiController] // dizer q é api e para ter funções de uma api 
[Route("api/[controller]")] // defenimos tb que tem q ter api
public class PolytechnicsController : ControllerBase
{
    private const string GetListPolytechnicsRout = "list";
    private const string GetPolytechnicsCountRout = "count";
    private const string GetPolytechnicsSortedRout = "sorted";
    private const string GetPolytechnicsSearchRout = "search";
    private readonly HttpClient httpClient;
    public PolytechnicsController(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    // curl "http://localhost:5276/api/polytechnics/test"
    [HttpGet("test")] // Route definition
    public async Task<ActionResult<Polytechnics>> GetPolytechnicstest()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<Polytechnics>>("http://universities.hipolabs.com/search?country=portugal");

            if (response == null || !response.Any())
                return NotFound("Nothing found");
            var polytechnic = response.FirstOrDefault();

            return Ok(polytechnic);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error fetching data from the API | Line 42");
        }
    }
    // curl "http://localhost:5276/api/polytechnics/list"
    [HttpGet(GetListPolytechnicsRout)] // adicionar o Route 
    public async Task<ActionResult<List<Polytechnics>>> GetPolytechnics()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<JsonElement>>("http://universities.hipolabs.com/search?country=portugal");

            if (response == null || !response.Any())
                return NotFound("No polytechnic found");
            var polytechnic = response
                .Where(element =>
                {
                    var name = element.GetProperty("name").GetString();
                    return name != null && (name.ToLower().Contains("politécnico") || name.ToLowerInvariant().Contains("politecnico"));
                })
                .Select(element => new Polytechnics
                {
                    Name = element.GetProperty("name").GetString() ?? "",
                    Webpage = element.GetProperty("web_pages")
                                .EnumerateArray()
                                .Select(url => url.GetString())
                                .ToList()
                })
                .ToList();
            return Ok(polytechnic);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error fetching data from the API");
        }
    }
    // curl "http://localhost:5276/api/polytechnics/count"
    [HttpGet(GetPolytechnicsCountRout)]
    public async Task<ActionResult<int>> GetPolytechnicsListCount()
    {
        List<Polytechnics> foundItems = GetPolytechnicsList(await GetPolytechnics());
        return Ok(foundItems.Count());
    }

    // curl "http://localhost:5276/api/polytechnics/sorted?ascending=false"
    [HttpGet(GetPolytechnicsSortedRout)]
    public async Task<ActionResult<List<Polytechnics>>> GetSortedPolytechnics([FromQuery] bool ascending = true)
    {
        List<Polytechnics> foundItems = GetPolytechnicsList(await GetPolytechnics());
        foundItems = ascending ? foundItems.OrderBy(i => i.Name).ToList() : foundItems.OrderByDescending(i => i.Name).ToList();
        return Ok(foundItems);
    }
    // curl "http://localhost:5276/api/polytechnics/search?name=beja"
    [HttpGet(GetPolytechnicsSearchRout)]
    public async Task<ActionResult<List<Polytechnics>>> SearchPolytechnics([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("| = You need to give a query, cannot be empty = |");
        List<Polytechnics> foundItems = GetPolytechnicsList(await GetPolytechnics())
            .Where(i => i.Name.ToLower().Contains(name.ToLower()))
            .ToList();
        if (!foundItems.Any()) return NotFound($"No Polyechnics were found with the name: '{name}'");
        return Ok(foundItems);
    }
    public List<Polytechnics> GetPolytechnicsList(ActionResult<List<Polytechnics>>? response)
    {
        List<Polytechnics> polytechnicsList = new List<Polytechnics>();

        if (response != null)
        {
            if (response.Result is OkObjectResult okResult)
            {
                var polytechnics = okResult.Value as List<Polytechnics>;
                if (polytechnics != null)
                {
                    polytechnicsList.AddRange(polytechnics);
                }
            }
            else
            {
                Console.WriteLine("Error at GetPolytechnics => OkObjectResult.");
            }
        }
        else
        {
            Console.WriteLine("reponse = null???");
        }
        return polytechnicsList;
    }
}