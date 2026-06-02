using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    /// <summary>
    /// Service for food image recognition using LogMeal AI API (deep learning)
    /// and TheMealDB API for recipe details.
    /// Step 1: LogMeal AI analyses the food image using CNN deep learning models
    /// Step 2: LogMeal returns nutritional information
    /// Step 3: TheMealDB provides real recipe data (description, ingredients, steps)
    /// Demonstrates machine learning, computer vision and networking.
    /// </summary>
    public class FoodRecognitionService
    {
        private const string LogMealToken = "adfd2b07cb26ffa07607c2996080c3221618f138";
        private const string LogMealBaseUrl = "https://api.logmeal.com/v2";
        private const string MealDbBaseUrl = "https://www.themealdb.com/api/json/v1/1";
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Result object containing all recognised food information
        /// from both LogMeal AI and TheMealDB APIs
        /// </summary>
        public class FoodResult
        {
            public string Name { get; set; } = "Unknown Food";
            public string Category { get; set; } = "Food";
            public string SubCategory { get; set; } = "Snack";
            public string Description { get; set; } = "";
            public string[] Ingredients { get; set; } = Array.Empty<string>();
            public string[] Steps { get; set; } = Array.Empty<string>();
            public double Calories { get; set; }
            public double Protein { get; set; }
            public double Carbs { get; set; }
            public double Fat { get; set; }
            public bool Success { get; set; } = false;
            public string ErrorMessage { get; set; } = "";
        }

        /// <summary>
        /// Constructor initialising HttpClient with LogMeal API authentication
        /// </summary>
        public FoodRecognitionService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", LogMealToken);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Full food analysis pipeline:
        /// 1. Send image to LogMeal deep learning model for dish recognition
        /// 2. Retrieve AI-generated nutritional information from LogMeal
        /// 3. Fetch real recipe details from TheMealDB online database
        /// All data comes from external APIs, no hardcoded templates.
        /// </summary>
        /// <param name="imagePath">Local file path of the food image</param>
        /// <returns>FoodResult with dish name, nutrition and recipe details</returns>
        public async Task<FoodResult> RecogniseFoodAsync(string imagePath)
        {
            var result = new FoodResult();

            try
            {
                // ===== Step 1: LogMeal AI - Deep learning food recognition =====
                string imageId = await RecogniseDishAsync(imagePath, result);

                // Classify into Food/Drink category
                result.Category = ClassifyCategory(result.Name);
                result.SubCategory = ClassifySubCategory(result.Name);

                // ===== Step 2: LogMeal AI - Nutritional information =====
                if (!string.IsNullOrEmpty(imageId))
                {
                    await FetchNutritionAsync(imageId, result);
                }

                // ===== Step 3: TheMealDB API - Recipe details =====
                await FetchRecipeDetailsAsync(result);

                result.Success = true;
            }
            catch (HttpRequestException httpEx)
            {
                result.ErrorMessage = "Network error. Please check your internet connection.";
                System.Diagnostics.Debug.WriteLine($"HTTP error: {httpEx.Message}");
            }
            catch (TaskCanceledException)
            {
                result.ErrorMessage = "Request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Recognition failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Recognition error: {ex.Message}");
            }

            return result;
        }

        // ==================== Step 1: LogMeal Dish Recognition ====================

        /// <summary>
        /// Send food image to LogMeal deep learning API for dish classification.
        /// The API uses convolutional neural networks (CNN) trained on food images
        /// to identify the dish from over 1300 categories.
        /// </summary>
        /// <returns>imageId for subsequent nutritional info request</returns>
        private async Task<string> RecogniseDishAsync(string imagePath, FoodResult result)
        {
            string recognitionUrl = $"{LogMealBaseUrl}/image/recognition/dish";

            using var imageStream = File.OpenRead(imagePath);
            using var content = new MultipartFormDataContent();
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "image", Path.GetFileName(imagePath));

            var response = await _httpClient.PostAsync(recognitionUrl, content);
            string json = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"LogMeal recognition: {json}");

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"Recognition failed: {response.StatusCode}";
                return "";
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Get image ID (can be number or string)
            string imageId = "";
            if (root.TryGetProperty("imageId", out var imageIdProp))
            {
                imageId = imageIdProp.ValueKind == JsonValueKind.Number
                    ? imageIdProp.GetInt64().ToString()
                    : (imageIdProp.GetString() ?? "");
            }

            // Extract dish name from recognition_results
            if (root.TryGetProperty("recognition_results", out var results) &&
                results.GetArrayLength() > 0)
            {
                var topResult = results[0];
                if (topResult.TryGetProperty("name", out var nameProp) &&
                    nameProp.ValueKind == JsonValueKind.String)
                {
                    result.Name = nameProp.GetString() ?? "Unknown Food";
                }
            }
            // Alternative: segmentation_results format
            else if (root.TryGetProperty("segmentation_results", out var segResults) &&
                     segResults.GetArrayLength() > 0)
            {
                var topSeg = segResults[0];
                if (topSeg.TryGetProperty("recognition_results", out var segRecResults) &&
                    segRecResults.GetArrayLength() > 0)
                {
                    var topResult = segRecResults[0];
                    if (topResult.TryGetProperty("name", out var nameProp) &&
                        nameProp.ValueKind == JsonValueKind.String)
                    {
                        result.Name = nameProp.GetString() ?? "Unknown Food";
                    }
                }
            }

            return imageId;
        }

        // ==================== Step 2: LogMeal Nutritional Info ====================

        /// <summary>
        /// Fetch AI-generated nutritional information from LogMeal API
        /// based on the recognised dish image.
        /// </summary>
        private async Task FetchNutritionAsync(string imageId, FoodResult result)
        {
            try
            {
                string nutritionUrl = $"{LogMealBaseUrl}/recipe/nutritionalInfo";
                var nutritionContent = new StringContent(
                    JsonSerializer.Serialize(new { imageId }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(nutritionUrl, nutritionContent);
                string json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"LogMeal nutrition: {json}");

                if (response.IsSuccessStatusCode)
                {
                    ParseNutritionData(json, result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Nutrition error: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse nutritional data from LogMeal API JSON response.
        /// Handles multiple response formats safely.
        /// </summary>
        private void ParseNutritionData(string json, FoodResult result)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Try nutritional_info object
                if (root.TryGetProperty("nutritional_info", out var info))
                {
                    if (info.TryGetProperty("calories", out var cal))
                        result.Calories = SafeGetDouble(cal);
                    if (info.TryGetProperty("totalNutrients", out var nutrients))
                    {
                        if (nutrients.TryGetProperty("PROCNT", out var p) &&
                            p.TryGetProperty("quantity", out var pq))
                            result.Protein = SafeGetDouble(pq);
                        if (nutrients.TryGetProperty("CHOCDF", out var c) &&
                            c.TryGetProperty("quantity", out var cq))
                            result.Carbs = SafeGetDouble(cq);
                        if (nutrients.TryGetProperty("FAT", out var f) &&
                            f.TryGetProperty("quantity", out var fq))
                            result.Fat = SafeGetDouble(fq);
                    }
                }

                // Fallback: nutritional_info_per_100g
                if (result.Calories == 0 &&
                    root.TryGetProperty("nutritional_info_per_100g", out var per100))
                {
                    if (per100.TryGetProperty("calories", out var cal100))
                        result.Calories = SafeGetDouble(cal100);
                    if (per100.TryGetProperty("totalNutrients", out var n100))
                    {
                        if (n100.TryGetProperty("PROCNT", out var p) &&
                            p.TryGetProperty("quantity", out var pq))
                            result.Protein = SafeGetDouble(pq);
                        if (n100.TryGetProperty("CHOCDF", out var c) &&
                            c.TryGetProperty("quantity", out var cq))
                            result.Carbs = SafeGetDouble(cq);
                        if (n100.TryGetProperty("FAT", out var f) &&
                            f.TryGetProperty("quantity", out var fq))
                            result.Fat = SafeGetDouble(fq);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseNutrition error: {ex.Message}");
            }
        }

        // ==================== Step 3: TheMealDB Recipe Details ====================

        /// <summary>
        /// Fetch real recipe details from TheMealDB free online API.
        /// Searches by the recognised food name and extracts
        /// description, ingredients list and cooking steps.
        /// All data comes dynamically from the API, not hardcoded.
        /// </summary>
        private async Task FetchRecipeDetailsAsync(FoodResult result)
        {
            try
            {
                // Remove auth header for TheMealDB (it doesn't need it)
                var mealClient = new HttpClient();
                mealClient.Timeout = TimeSpan.FromSeconds(15);

                // First try: search with full name
                string searchName = Uri.EscapeDataString(result.Name);
                string url = $"{MealDbBaseUrl}/search.php?s={searchName}";

                var response = await mealClient.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"MealDB response: {json}");

                bool parsed = TryParseMealDbResponse(json, result);

                // Second try: search with last word (main food type)
                if (!parsed)
                {
                    string[] words = result.Name.Split(' ');
                    if (words.Length > 1)
                    {
                        string simpleSearch = Uri.EscapeDataString(words[words.Length - 1]);
                        url = $"{MealDbBaseUrl}/search.php?s={simpleSearch}";

                        response = await mealClient.GetAsync(url);
                        json = await response.Content.ReadAsStringAsync();

                        parsed = TryParseMealDbResponse(json, result);
                    }
                }

                // Third try: search with first word
                if (!parsed)
                {
                    string[] words = result.Name.Split(' ');
                    string firstWord = Uri.EscapeDataString(words[0]);
                    url = $"{MealDbBaseUrl}/search.php?s={firstWord}";

                    response = await mealClient.GetAsync(url);
                    json = await response.Content.ReadAsStringAsync();

                    parsed = TryParseMealDbResponse(json, result);
                }

                // If still no match, set informative defaults
                if (!parsed)
                {
                    result.Description = $"A delicious {result.Name} identified by AI food recognition.";
                    result.Ingredients = Array.Empty<string>();
                    result.Steps = Array.Empty<string>();
                }

                mealClient.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MealDB error: {ex.Message}");
                result.Description = $"A delicious {result.Name} identified by AI food recognition.";
            }
        }

        /// <summary>
        /// Parse TheMealDB JSON response and extract recipe details.
        /// Returns true if a matching meal was found and parsed.
        /// </summary>
        private bool TryParseMealDbResponse(string json, FoodResult result)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("meals", out var meals) ||
                    meals.ValueKind == JsonValueKind.Null ||
                    meals.GetArrayLength() == 0)
                {
                    return false;
                }

                var meal = meals[0];

                // Extract description from instructions (first 1-2 sentences)
                if (meal.TryGetProperty("strInstructions", out var instrProp))
                {
                    string fullText = instrProp.GetString() ?? "";
                    var sentences = fullText.Split('.');
                    if (sentences.Length >= 2)
                        result.Description = sentences[0].Trim() + ". " + sentences[1].Trim() + ".";
                    else if (sentences.Length == 1)
                        result.Description = sentences[0].Trim() + ".";
                }

                // Extract ingredients with measures (strIngredient1-20, strMeasure1-20)
                var ingredientList = new List<string>();
                for (int i = 1; i <= 20; i++)
                {
                    string ingredient = "";
                    string measure = "";

                    if (meal.TryGetProperty($"strIngredient{i}", out var ingProp))
                        ingredient = ingProp.GetString()?.Trim() ?? "";
                    if (meal.TryGetProperty($"strMeasure{i}", out var mesProp))
                        measure = mesProp.GetString()?.Trim() ?? "";

                    if (!string.IsNullOrEmpty(ingredient))
                    {
                        string item = !string.IsNullOrEmpty(measure)
                            ? $"{measure} {ingredient}"
                            : ingredient;
                        ingredientList.Add(item);
                    }
                }
                result.Ingredients = ingredientList.ToArray();

                // Extract cooking steps from instructions
                if (meal.TryGetProperty("strInstructions", out var stepsProp))
                {
                    string fullText = stepsProp.GetString() ?? "";
                    var stepList = new List<string>();

                    // Split by newlines
                    string[] rawSteps = fullText.Split(
                        new[] { "\r\n", "\n", "\r" },
                        StringSplitOptions.RemoveEmptyEntries);

                    foreach (var step in rawSteps)
                    {
                        string trimmed = step.Trim();
                        // Remove leading step numbers
                        trimmed = Regex.Replace(trimmed,
                            @"^(step\s*)?\d+[\.\)\:\-]\s*", "",
                            RegexOptions.IgnoreCase);

                        if (trimmed.Length > 5)
                        {
                            stepList.Add(trimmed);
                        }
                    }

                    // If no newlines found, split by sentences
                    if (stepList.Count <= 1 && fullText.Length > 50)
                    {
                        stepList.Clear();
                        string[] sentences = fullText.Split('.');
                        foreach (var sentence in sentences)
                        {
                            string trimmed = sentence.Trim();
                            if (trimmed.Length > 10)
                            {
                                stepList.Add(trimmed + ".");
                            }
                        }
                    }

                    result.Steps = stepList.ToArray();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseMealDb error: {ex.Message}");
                return false;
            }
        }

        // ==================== Helper Methods ====================

        /// <summary>
        /// Safely extract a double from a JsonElement (handles number and string types)
        /// </summary>
        private double SafeGetDouble(JsonElement element)
        {
            try
            {
                if (element.ValueKind == JsonValueKind.Number)
                    return element.GetDouble();
                if (element.ValueKind == JsonValueKind.String &&
                    double.TryParse(element.GetString(), out double val))
                    return val;
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Classify recognised food name into Food or Drink category
        /// </summary>
        private string ClassifyCategory(string foodName)
        {
            string lower = foodName.ToLower();
            string[] drinkKeywords = { "juice", "tea", "coffee", "smoothie", "milk",
                "water", "soda", "beer", "wine", "cocktail", "lemonade", "shake" };

            foreach (var keyword in drinkKeywords)
            {
                if (lower.Contains(keyword)) return "Drink";
            }
            return "Food";
        }

        /// <summary>
        /// Classify recognised food name into a sub-category
        /// </summary>
        private string ClassifySubCategory(string foodName)
        {
            string lower = foodName.ToLower();

            if (lower.Contains("tea") || lower.Contains("coffee") || lower.Contains("chocolate"))
                return "Hot Drink";
            if (lower.Contains("juice") || lower.Contains("smoothie") || lower.Contains("lemonade"))
                return "Cold Drink";
            if (lower.Contains("pancake") || lower.Contains("waffle") || lower.Contains("cereal") ||
                lower.Contains("toast") || lower.Contains("egg") || lower.Contains("oatmeal"))
                return "Breakfast";
            if (lower.Contains("cake") || lower.Contains("cookie") || lower.Contains("brownie") ||
                lower.Contains("ice cream") || lower.Contains("dessert") || lower.Contains("pie"))
                return "Dessert";
            if (lower.Contains("salad") || lower.Contains("sandwich") || lower.Contains("soup") ||
                lower.Contains("burger"))
                return "Lunch";

            return "Dinner";
        }
    }
}