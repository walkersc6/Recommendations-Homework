using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class RecommendationController : Controller
{
    private readonly HttpClient _httpClient;

    public RecommendationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    private async Task<List<int>> GetAzureMLRecommendations(int idValue, string idType)
    {
        // Call the Azure ML Endpoint (adjust endpoint and API key as needed)
        var endpoint = "https://<your-azure-ml-endpoint>";
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(endpoint),
            Content = new StringContent($"{{\"ID\":{idValue}}}", System.Text.Encoding.UTF8, "application/json")
        };

        // Add Azure API Key (use environment variables for security)
        request.Headers.Add("Authorization", "Bearer <Your_Azure_ML_API_Key>");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var predictionData = JsonConvert.DeserializeObject<List<int>>(responseString); // assuming Azure returns a list of item IDs

        return predictionData ?? new List<int>();
    }
}

