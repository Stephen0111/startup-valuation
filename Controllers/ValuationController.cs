
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Startup.Models; // Ensure this is present and correct
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Startup.Controllers
{
   
    public class GroqChatCompletionResponse
    {
        // Suppress nullable warnings for DTOs
        public Choice[] choices { get; set; } = Array.Empty<Choice>();
    }

    public class Choice
    {
        public Message message { get; set; } = new Message();
    }

    public class Message
    {
        public string content { get; set; } = string.Empty;
    }
    // --- End Helper Models ---


    [ApiController]
    [Route("api/[controller]")]
    public class ValuationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constants for API Configuration
        private const string GroqModel = "llama-3.1-8b-instant";
        private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

        public ValuationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] StartupValuationRequest request)
        {
            if (string.IsNullOrEmpty(request.CompanyName))
                return BadRequest(new { error = "Company name is required." });

            // Define the System Prompt to enforce JSON output
            var systemPrompt = "You are a financial AI assistant. STRICTLY return ONLY a JSON object that matches the requested schema. DO NOT include any explanatory text or markdown outside the JSON.";

            // Build the detailed User Prompt
            // NOTE: The prompt is updated to be stricter about the 'Valuation' field being a clean number.
            var userPrompt = $@"
                Evaluate this startup and return JSON with numeric valuation and analysis.
                Company: {request.CompanyName}
                Industry: {request.Industry}
                Stage: {request.Stage}
                Founding Year: {request.FoundingYear}
                Monthly Revenue: {request.MonthlyRevenue}
                Growth Rate: {request.RevenueGrowthRate}
                Expenses: {request.MonthlyExpenses}
                Funding: {request.FundingRaised}
                Burn Rate: {request.BurnRate}
                Months to Breakeven: {request.MonthsToBreakeven}
                Customers: {request.CustomersCount}
                Team Size: {request.TeamSize}
                Market Size: {request.MarketSize}
                IP: {request.IntellectualProperty}
                Competitor Valuation: {request.CompetitorValuation}
                CAC: {request.CustomerAcquisitionCost}
                LTV: {request.LifetimeValue}

                Return ONLY a raw JSON object with no surrounding text or markdown.
                The 'Valuation' MUST be a clean integer number (e.g., 5000000) with NO commas, NO currency symbols, and NO extra text.
                {{ ""Valuation"": <clean number>, ""Analysis"": ""<short summary of method>"" }}
            ";

            try
            {
                var client = _httpClientFactory.CreateClient();
                
                client.DefaultRequestHeaders.Add("Authorization", "API-KEY");

                var payload = new
                {
                    model = GroqModel, 
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt } // Use the detailed prompt
                    },
                    response_format = new { type = "json_object" }
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(GroqUrl, content);
                var rawText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = "Groq API Error: " + rawText });

                // Correctly deserialize the Groq API response envelope
                var apiResponse = JsonConvert.DeserializeObject<GroqChatCompletionResponse>(rawText);
    
                if (apiResponse?.choices == null || apiResponse.choices.Length == 0)
                    return BadRequest(new { error = "Empty or invalid response from Groq API." });

                // The actual clean JSON is in the first choice's message content
                var jsonPart = apiResponse.choices[0].message.content;

                
                // This handles cases where the model wraps the JSON in code fences (```json ... ``` or ``` ... ```)
                var trimmedContent = jsonPart.Trim();
                if (trimmedContent.StartsWith("```"))
                {
                    // Look for the first actual open brace '{' and the last closing brace '}'
                    var firstBraceIndex = trimmedContent.IndexOf('{');
                    var lastBraceIndex = trimmedContent.LastIndexOf('}');
                    
                    if (firstBraceIndex != -1 && lastBraceIndex != -1 && lastBraceIndex > firstBraceIndex)
                    {
                        // Extract only the content between the first { and the last }
                        jsonPart = trimmedContent.Substring(firstBraceIndex, lastBraceIndex - firstBraceIndex + 1);
                    }
                    else
                    {
                        // Fallback: simple stripping of common markdown fences
                        jsonPart = trimmedContent.Replace("```json", "").Replace("```", "").Trim();
                    }
                }
                // Simple final check for surrounding whitespace
                jsonPart = jsonPart.Trim();
                // ------------------------------------------------------------------

                // Parse JSON safely
                var parsed = JsonConvert.DeserializeObject<StartupValuationResponse>(jsonPart);

                if (parsed == null)
                    return BadRequest(new { error = "Could not parse final JSON from AI content after cleaning." });

                // Ensure defaults if AI returned nulls
                parsed.Analysis ??= "No analysis provided by AI.";

                return Ok(parsed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error: " + ex.Message });
            }
        }
    }
}
