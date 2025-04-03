using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Formats.Asn1;
using System.Globalization;
using System;
using Recommendations.Models;
using CsvHelper;
using CsvHelper.Configuration;

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
    public async Task<IActionResult> GetRecommendations(double idValue, string idType)
    {
        // Assume GetCollaborativeRecommendations is defined elsewhere in the controller
        var collaborativeRecommendations = await GetCollaborativeRecommendations(idValue, idType);
        //var contentFilteringRecommendations = await GetContentFilteringRecommendations(idValue, idType);
        //var azureMlRecommendations = await GetAzureMLRecommendations(idValue, idType);

        // Passing recommendations to ViewData
        ViewData["Recommendations"] = new List<List<double>>
    {
        collaborativeRecommendations,
        //contentFilteringRecommendations,
        //azureMlRecommendations
    };

        return View("Index");
    }

    private async Task<List<int>> GetContentFilteringRecommendations(int idValue, string idType)
    {
        throw new NotImplementedException();
    }


    //[HttpPost]
    //public async Task<IActionResult> GetRecommendations(int idValue, string idType)
    //{

    //    // Lists to store recommended item IDs
    //    List<List<int>> recommendations = new List<List<int>>();

    //    // Get recommendations from Collaborative Filtering (Python)
    //    var collaborativeRecommendations = await GetCollaborativeRecommendations(idValue, idType);
    //    recommendations.Add(collaborativeRecommendations);

    //    // Get recommendations from Content Filtering (Python)
    //    var contentRecommendations = await GetContentRecommendations(idValue, idType);
    //    recommendations.Add(contentRecommendations);

    //    // Get recommendations from Azure ML
    //    //var azureRecommendations = await GetAzureMLRecommendations(idValue, idType);
    //    //recommendations.Add(azureRecommendations);

    //    // Pass recommendations to View
    //    ViewData["Recommendations"] = recommendations;


    //    return View("Index");
    //}

    private async Task<List<double>> GetCollaborativeRecommendations(double idValue, string idType)
    {
        // Path to the CSV file in the App_Data folder
        string filePath = "App_Data/collaborative_filtering_results.csv";

        Collab recs = null;

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",         // Specify tab as the delimiter
            HeaderValidated = null,   // Disable header validation (if headers are messy)
            MissingFieldFound = null, // If fields are missing, we won't throw errors
            //TrimOptions = TrimOptions.Trim, // Automatically trim fields (values)
        }))
        {
            // Register the class map for the Collab class
            csv.Context.RegisterClassMap<CollabMap>();
            var records = csv.GetRecords<Collab>().ToList();
            recs = records.FirstOrDefault(p => p.contentId.ToString() == idValue.ToString());

            //recs = records.FirstOrDefault(p => p.contentId == idValue);
        }

        // Check if a person was found, and pass to the view
        if (recs != null)
        {
            Console.WriteLine($"contentId: {recs.contentId}");
            Console.WriteLine($"IfYouRead: {recs.IfYouRead}");
            Console.WriteLine($"Recommendation1: {recs.Recommendation1}");
            Console.WriteLine($"Recommendation2: {recs.Recommendation2}");
            Console.WriteLine($"Recommendation3: {recs.Recommendation3}");
            Console.WriteLine($"Recommendation4: {recs.Recommendation4}");
            Console.WriteLine($"Recommendation5: {recs.Recommendation5}");
            var collabRec = new List<double>
            {
                recs.contentId,
                recs.IfYouRead,
                recs.Recommendation1,
                recs.Recommendation2,
                recs.Recommendation3,
                recs.Recommendation4,
                recs.Recommendation5
            };
            return collabRec;
        }
        else
        {
            ViewBag.Message = "Person not found!";
            return new List<double>();  // Return an empty list if not found
        }
    }

    //private async Task<List<int>> GetContentRecommendations(int idValue, string idType)
    //{
    //    // Simulate API call to Python-based content filtering model
    //    // You can call an actual API here that serves content-based recommendations
    //    return await Task.FromResult(new List<int> { 201, 202, 203, 204, 205 });
    //}

    //private async Task<List<int>> GetAzureMLRecommendations(int userId, string idType)
    //{
    //    var endpoint = "https://<your-region>.inference.ml.azure.com/score"; // Your real URL
    //    var apiKey = "<your-azure-api-key>"; // 🔒 Store this securely later!

    //    var payload = new
    //    {
    //        Inputs = new
    //        {
    //            userId = userId // Change to match Azure’s expected schema
    //        }
    //    };

    //    var json = JsonConvert.SerializeObject(payload);
    //    var request = new HttpRequestMessage
    //    {
    //        Method = HttpMethod.Post,
    //        RequestUri = new Uri(endpoint),
    //        Content = new StringContent(json, Encoding.UTF8, "application/json")
    //    };

    //    request.Headers.Add("Authorization", $"Bearer {apiKey}`);

    //    var response = await _httpClient.SendAsync(request);
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        // Optional: log or handle error gracefully
    //        return new List<int>();
    //    }

    //    var responseString = await response.Content.ReadAsStringAsync();

    //    // If Azure returns a JSON array like [1001, 1002, 1003, 1004, 1005]
    //    var predictionData = JsonConvert.DeserializeObject<List<int>>(responseString);

    //    return predictionData ?? new List<int>();
    //}
}

