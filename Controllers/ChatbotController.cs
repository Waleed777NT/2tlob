using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace _2tlob.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ChatbotController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return Json(new { reply = "Please enter a valid message." });
            }

            string apiKey = _configuration["Gemini:ApiKey"] ?? string.Empty;

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Gemini API key is missing in application configuration.");
                return Json(new { reply = GetRuleBasedResponse(request.Message) });
            }

            try
            {
                //string systemPrompt = "You are an AI customer support assistant for '2tlob' e-commerce store in Egypt. Be concise, friendly, and answer in the same language as the user. User message: ";

                var requestBody = new
                {
                    systemInstruction = new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = """
                                       You are the customer support assistant for 2tlob, an e-commerce store in Egypt.

                                       Rules:
                                       - Answer in the same language as the user.
                                       - Be concise, friendly, and helpful.
                                       - Do not invent information about products, orders, prices, delivery, or policies.
                                       - If you don't know something, clearly say that you don't know.
                                       - You can explain how users can use the website.
                                       """
                            }
                        }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = request.Message }
                            }
                        }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                string modelName = "gemini-3.5-flash-lite";

                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}",
                    jsonContent
                );

                var responseString = await response.Content.ReadAsStringAsync();

                // for console check
                Console.WriteLine("STATUS: " + response.StatusCode);
                Console.WriteLine("RESPONSE: " + responseString);

                if ((int)response.StatusCode == 503)
                {
                    return Json(new
                    {
                        reply = "I'm currently unable to connect to the AI assistant. Please try again in a moment."
                    });
                }

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    var aiReply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (!string.IsNullOrEmpty(aiReply))
                    {
                        return Json(new { reply = aiReply });
                    }
                }
                else
                {
                    _logger.LogError("Gemini API call failed with StatusCode {StatusCode}: {ErrorDetails}",
                        response.StatusCode, responseString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the chatbot request.");
            }

            return Json(new { reply = GetRuleBasedResponse(request.Message) });
        }

        private string GetRuleBasedResponse(string input)
        {
            string lowerInput = input.ToLower().Trim();

            if (lowerInput.Contains("track") || lowerInput.Contains("order") || lowerInput.Contains("تتبع") || lowerInput.Contains("طلب"))
            {
                return "To track your order, go to your Profile -> Orders page to see live status updates.";
            }
            if (lowerInput.Contains("return") || lowerInput.Contains("refund") || lowerInput.Contains("استرجاع") || lowerInput.Contains("إرجاع"))
            {
                return "Our return policy allows items to be returned within 14 days of delivery if unused.";
            }
            if (lowerInput.Contains("product") || lowerInput.Contains("search") || lowerInput.Contains("منتج") || lowerInput.Contains("بحث"))
            {
                return "You can search for products using the search bar at the top of the navbar or explore by Categories.";
            }

            return "Hello! How can I assist you with your shopping on 2tlob today?";
        }
    }
}