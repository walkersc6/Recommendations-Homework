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
using System.Diagnostics;

public class RecommendationController : Controller
{
    private readonly HttpClient _httpClient;

    public RecommendationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> IndexAsync()
    {
        // Get the content IDs from CSVs
        var contentIds = await GetContentIdsFromCsvs();

        // Pass the content IDs to the View
        ViewData["ContentIds"] = contentIds;

        return View();
    }

    private async Task<List<decimal>> GetContentIdsFromCsvs()
    {
        string filePath1 = "App_Data/collaborative_filtering_results.csv"; // Path to your first CSV file
        string filePath2 = "App_Data/content_filtering_results.csv"; // Path to your second CSV file

        List<decimal> contentIds = new List<decimal>();

        using (var reader1 = new StreamReader(filePath1))
        using (var csv1 = new CsvReader(reader1, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null
        }))
        {
            var records1 = csv1.GetRecords<dynamic>().ToList();
            foreach (var record in records1)
            {
                Debug.WriteLine($"Record contentId (first file): {record.contentId}");
                decimal contentId;
                if (decimal.TryParse(record.contentId.ToString(), out contentId))
                {
                    contentIds.Add(contentId);
                }
            }
        }

        // Read the second CSV file
        using (var reader2 = new StreamReader(filePath2))
        using (var csv2 = new CsvReader(reader2, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null
        }))
        {
            var records2 = csv2.GetRecords<dynamic>().ToList();
            foreach (var record in records2)
            {
                decimal contentId;
                if (decimal.TryParse(record.contentId.ToString(), out contentId))
                {
                    contentIds.Add(contentId);
                }
            }
        }

        return contentIds.Distinct().ToList(); // Removing duplicates if any
    }

    //private async Task<List<double>> GetContentIdsFromCsvs()
    //{
    //    string filePath1 = "App_Data/collaborative_filtering_results.csv"; // Path to your first CSV file
    //    string filePath2 = "App_Data/content_filtering_results.csv"; // Path to your second CSV file

    //    List<double> contentIds = new List<double>();

    //    // Read the first CSV file
    //    using (var reader1 = new StreamReader(filePath1))
    //    using (var csv1 = new CsvReader(reader1, new CsvConfiguration(CultureInfo.InvariantCulture)
    //    {
    //        Delimiter = ",",
    //        HeaderValidated = null,
    //        MissingFieldFound = null
    //    }))
    //    {
    //        var records1 = csv1.GetRecords<dynamic>().ToList();
    //        foreach (var record in records1)
    //        {
    //            double contentId;
    //            // Access contentId directly using the dynamic property
    //            Console.WriteLine(record.contentId);
    //            if (double.TryParse(record.contentId.ToString(), out contentId))
    //            {
    //                contentIds.Add(contentId);
    //            }
    //        }
    //    }

    //    // Read the second CSV file
    //    using (var reader2 = new StreamReader(filePath2))
    //    using (var csv2 = new CsvReader(reader2, new CsvConfiguration(CultureInfo.InvariantCulture)
    //    {
    //        Delimiter = ",",
    //        HeaderValidated = null,
    //        MissingFieldFound = null
    //    }))
    //    {
    //        var records2 = csv2.GetRecords<dynamic>().ToList();
    //        foreach (var record in records2)
    //        {
    //            double contentId;
    //            // Access contentId directly using the dynamic property
    //            if (double.TryParse(record.contentId.ToString(), out contentId))
    //            {
    //                contentIds.Add(contentId);
    //            }
    //        }
    //    }

    //    return contentIds.Distinct().ToList(); // Removing duplicates if any
    //}


    [HttpPost]
    public async Task<IActionResult> GetRecommendations(double idValue, string idType)
    {
        // Assume GetCollaborativeRecommendations is defined elsewhere in the controller
        var collaborativeRecommendations = await GetCollaborativeRecommendations(idValue, idType);
        var contentFilteringRecommendations = await GetContentRecommendations(idValue, idType);
        //var azureMlRecommendations = await GetAzureMLRecommendations(idValue, idType);

        //get the list of contentId
        var contentIds = await GetContentIdsFromCsvs();

        var formattedCollaborativeRecommendations = collaborativeRecommendations.Select(r => r.ToString("F0")).ToList();
        var formattedContentFilteringRecommendations = contentFilteringRecommendations.Select(r => r.ToString("F0")).ToList();
        var formattedAzureMlRecommendations = new List<string>(); // Assuming no data for now

        // Passing recommendations to ViewData
        ViewData["Recommendations"] = new List<List<string>>
    {
        formattedCollaborativeRecommendations,
        formattedContentFilteringRecommendations,
        formattedAzureMlRecommendations
    };

        //    // Passing recommendations to ViewData
        //    ViewData["Recommendations"] = new List<List<double>>
        //{
        //    collaborativeRecommendations,
        //    contentFilteringRecommendations, // placeholder for content filtering
        //    new List<double>()  // placeholder for Azure ML
        //};

        // Pass the contentIds for the dropdown in the view
        ViewData["ContentIds"] = contentIds;

        return View("Index");
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
    

    private async Task<List<double>> GetContentRecommendations(double idValue, string idType)
    {
        string filePath = "App_Data/content_filtering_results.csv";

        Content recs = null;

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",         // Specify tab as the delimiter
            HeaderValidated = null,   // Disable header validation (if headers are messy)
            MissingFieldFound = null, // If fields are missing, we won't throw errors
            //TrimOptions = TrimOptions.Trim, // Automatically trim fields (values)
        }))
        {
            // Register the class map for the Content class
            csv.Context.RegisterClassMap<ContentMap>();
            var records = csv.GetRecords<Content>().ToList();
            recs = records.FirstOrDefault(p => p.contentId == idValue);
        }

        // Check if a person was found, and pass to the view
        if (recs != null)
        {
            Console.WriteLine($"contentId: {recs.contentId}");
            Console.WriteLine($"Top1: {recs.Top1}");
            Console.WriteLine($"Top2: {recs.Top2}");
            Console.WriteLine($"Top3: {recs.Top3}");
            Console.WriteLine($"Top4: {recs.Top4}");
            Console.WriteLine($"Top5: {recs.Top5}");
            var contentRec = new List<double>
            {
                recs.Top1,
                recs.Top2,
                recs.Top3,
                recs.Top4,
                recs.Top5
            };
            return contentRec;
        }
        else
        {
            ViewBag.Message = "Person not found!";
            return new List<double>();  // Return an empty list if not found
        }
    }

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

