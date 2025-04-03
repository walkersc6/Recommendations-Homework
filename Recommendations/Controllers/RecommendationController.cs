using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class RecommendationController : Controller
{
    private readonly HttpClient _httpClient;

    public RecommendationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetRecommendations(int idValue, string idType)
    {
        // Lists to store recommended item IDs
        List<List<int>> recommendations = new List<List<int>>();

        // Get recommendations from Collaborative Filtering (Python)
        var collaborativeRecommendations = await GetCollaborativeRecommendations(idValue, idType);
        recommendations.Add(collaborativeRecommendations);

        // Get recommendations from Content Filtering (Python)
        var contentRecommendations = await GetContentRecommendations(idValue, idType);
        recommendations.Add(contentRecommendations);

        // Get recommendations from Azure ML
        var azureRecommendations = await GetAzureMLRecommendations(idValue, idType);
        recommendations.Add(azureRecommendations);

        // Pass recommendations to View
        ViewData["Recommendations"] = recommendations;

        return View();
    }

    private async Task<List<int>> GetCollaborativeRecommendations(int idValue, string idType)
    {
        // Simulate API call to Python-based collaborative filtering model
        // You can call an actual API here that serves collaborative recommendations
        return await Task.FromResult(new List<int> { 101, 102, 103, 104, 105 });
    }

    private async Task<List<int>> GetContentRecommendations(int idValue, string idType)
    {
        // Simulate API call to Python-based content filtering model
        // You can call an actual API here that serves content-based recommendations
        return await Task.FromResult(new List<int> { 201, 202, 203, 204, 205 });
    }

    private async Task<List<int>> GetAzureMLRecommendations(int userId, string idType)
    {
        var endpoint = "https://<your-region>.inference.ml.azure.com/score"; // Your real URL
        var apiKey = "<your-azure-api-key>"; // 🔒 Store this securely later!

        var payload = new
        {
            Inputs = new
            {
                userId = userId // Change to match Azure’s expected schema
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(endpoint),
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            // Optional: log or handle error gracefully
            return new List<int>();
        }

        var responseString = await response.Content.ReadAsStringAsync();

        // If Azure returns a JSON array like [1001, 1002, 1003, 1004, 1005]
        var predictionData = JsonConvert.DeserializeObject<List<int>>(responseString);

        return predictionData ?? new List<int>();
    }
}

