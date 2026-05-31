using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TasteHub.Models;
using TasteHub.Services;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Home page, handling recipe listing, search,
    /// category filtering, barometer recommendation and shake-to-earn coupon
    /// </summary>
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private bool _isLoading = false;

        /// <summary>Full collection of recipes displayed in the list</summary>
        public ObservableCollection<Recipe> Recipes { get; } = new();

        /// <summary>Currently selected category filter: All, Food, or Drink</summary>
        [ObservableProperty]
        private string _selectedCategory = "All";

        /// <summary>User search query text</summary>
        [ObservableProperty]
        private string _searchText = string.Empty;

        /// <summary>Barometer-based recommended recipe</summary>
        [ObservableProperty]
        private Recipe _recommendedRecipe;

        /// <summary>Whether a coupon popup should be displayed</summary>
        [ObservableProperty]
        private bool _showCouponPopup;

        /// <summary>Most recently earned coupon</summary>
        [ObservableProperty]
        private Coupon _latestCoupon;

        /// <summary>Current barometer pressure reading in hPa</summary>
        [ObservableProperty]
        private double _currentPressure;

        /// <summary>
        /// Constructor with dependency injection of database service
        /// </summary>
        public HomeViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "TasteHub";
        }

        /// <summary>
        /// Load all recipes from the database and apply current filters.
        /// Uses _isLoading flag to prevent concurrent loading.
        /// </summary>
        [RelayCommand]
        public async Task LoadRecipesAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                IsBusy = true;
                Recipes.Clear();

                var recipes = await _databaseService.GetAllRecipesAsync();

                // Apply category filter
                if (SelectedCategory != "All")
                {
                    recipes = recipes.Where(r => r.Category == SelectedCategory).ToList();
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string query = SearchText.ToLower();
                    recipes = recipes.Where(r =>
                        r.Name.ToLower().Contains(query) ||
                        r.Description.ToLower().Contains(query)).ToList();
                }

                foreach (var recipe in recipes)
                {
                    Recipes.Add(recipe);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to load recipes. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"LoadRecipes error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                _isLoading = false;
            }
        }

        /// <summary>
        /// Filter recipes when the search text changes
        /// </summary>
        partial void OnSearchTextChanged(string value)
        {
            LoadRecipesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Navigate to the recipe detail page
        /// </summary>
        [RelayCommand]
        public async Task GoToDetailAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                await Shell.Current.GoToAsync($"DetailPage?id={recipe.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to open recipe details. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"GoToDetail error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to the add recipe page
        /// </summary>
        [RelayCommand]
        public async Task GoToAddRecipeAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("AddEditPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to open add recipe page. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"GoToAddRecipe error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a recipe with confirmation dialog
        /// </summary>
        [RelayCommand]
        public async Task DeleteRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete",
                $"Are you sure you want to delete '{recipe.Name}'?", "Yes", "No");

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteRecipeAsync(recipe);
                    Recipes.Remove(recipe);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error",
                        "Failed to delete the recipe. Please try again.", "OK");
                    System.Diagnostics.Debug.WriteLine($"DeleteRecipe error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generate a random coupon when the user shakes the device
        /// </summary>
        [RelayCommand]
        public async Task ShakeDetectedAsync()
        {
            try
            {
                var random = new Random();
                var coupon = new Coupon
                {
                    Code = $"TASTE{random.Next(1000, 9999)}",
                    Description = $"{random.Next(1, 4) * 5}% off your next meal!",
                    Discount = random.Next(1, 4) * 5,
                    ObtainedAt = DateTime.Now
                };

                await _databaseService.SaveCouponAsync(coupon);
                LatestCoupon = coupon;
                ShowCouponPopup = true;

                await Shell.Current.DisplayAlert("Coupon Earned!",
                    $"Code: {coupon.Code}\n{coupon.Description}", "Awesome!");

                ShowCouponPopup = false;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to generate coupon. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"ShakeDetected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Recommend a recipe based on barometer pressure reading.
        /// Low pressure suggests warm food/drinks, high pressure suggests cold ones.
        /// </summary>
        [RelayCommand]
        public async Task UpdateBarometerRecommendationAsync(double pressure)
        {
            try
            {
                CurrentPressure = pressure;
                var allRecipes = await _databaseService.GetAllRecipesAsync();

                if (allRecipes.Count == 0) return;

                // Low pressure (< 1013 hPa) -> recommend hot items
                // High pressure (>= 1013 hPa) -> recommend cold items
                var filtered = pressure < 1013
                    ? allRecipes.Where(r =>
                        r.SubCategory == "Dinner" ||
                        r.SubCategory == "Hot Drink").ToList()
                    : allRecipes.Where(r =>
                        r.SubCategory == "Cold Drink" ||
                        r.SubCategory == "Lunch" ||
                        r.SubCategory == "Dessert").ToList();

                if (filtered.Count > 0)
                {
                    var random = new Random();
                    RecommendedRecipe = filtered[random.Next(filtered.Count)];
                }
                else
                {
                    var random = new Random();
                    RecommendedRecipe = allRecipes[random.Next(allRecipes.Count)];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Barometer recommendation error: {ex.Message}");
            }
        }
    }
}