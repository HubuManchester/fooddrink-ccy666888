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
    /// View model for the Home page, managing the recipe list, search,
    /// category filtering, barometer-based recommendation, shake-to-earn coupon
    /// and compass-driven Surprise Me feature.
    /// Demonstrates code reusability via shared BaseViewModel and IDatabaseService interface.
    /// </summary>
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        /// <summary>Guards against concurrent recipe list loads causing duplicates</summary>
        private bool _isLoading = false;

        /// <summary>Ensures barometer recommendation is set only once per page visit</summary>
        private bool _barometerInitialized = false;

        /// <summary>Observable collection of recipes shown in the list</summary>
        public ObservableCollection<Recipe> Recipes { get; } = new();

        /// <summary>Currently active category filter: All, Food, or Drink</summary>
        [ObservableProperty]
        private string _selectedCategory = "All";

        /// <summary>Real-time search text used to filter recipes by name or description</summary>
        [ObservableProperty]
        private string _searchText = string.Empty;

        /// <summary>Barometer-recommended recipe shown in the recommendation card</summary>
        [ObservableProperty]
        private Recipe _recommendedRecipe;

        /// <summary>Whether the coupon popup should be visible</summary>
        [ObservableProperty]
        private bool _showCouponPopup;

        /// <summary>The most recently generated coupon from a shake event</summary>
        [ObservableProperty]
        private Coupon _latestCoupon;

        /// <summary>Current atmospheric pressure in hPa from the barometer sensor</summary>
        [ObservableProperty]
        private double _currentPressure;

        /// <summary>Magnetic heading in degrees from the compass sensor, drives the wheel rotation</summary>
        [ObservableProperty]
        private double _compassHeading;

        /// <summary>Whether the Surprise Me compass wheel is currently spinning</summary>
        [ObservableProperty]
        private bool _isSpinning;

        /// <summary>Recipe selected by the compass Surprise Me feature</summary>
        [ObservableProperty]
        private Recipe _surpriseRecipe;

        /// <summary>Whether the Surprise Me result card is visible</summary>
        [ObservableProperty]
        private bool _showSurpriseResult;

        /// <summary>
        /// Constructor with dependency injection of database service
        /// </summary>
        /// <param name="databaseService">Injected database service for recipe and coupon operations</param>
        public HomeViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "TasteHub";
        }

        /// <summary>
        /// Load recipes from the database and apply the current category and search filters.
        /// Uses _isLoading guard to prevent duplicate concurrent loads.
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
        /// Automatically reload the recipe list whenever the search text changes
        /// </summary>
        partial void OnSearchTextChanged(string value)
        {
            LoadRecipesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Navigate to the recipe detail page
        /// </summary>
        /// <param name="recipe">Recipe to display in detail</param>
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
        /// Navigate directly to the edit page for a recipe via swipe action.
        /// Uses EditRecipePage route (same AddEditPage, registered separately) to
        /// avoid tab navigation conflicts — demonstrates code reusability.
        /// </summary>
        /// <param name="recipe">Recipe to edit</param>
        [RelayCommand]
        public async Task GoToEditAsync(Recipe recipe)
        {
            if (recipe == null) return;
            try
            {
                await Shell.Current.GoToAsync($"EditRecipePage?id={recipe.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GoToEdit error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to the Add Recipe page via the tab bar Add button
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
        /// Delete a recipe after confirming with the user.
        /// Shows a confirmation dialog to prevent accidental deletion.
        /// </summary>
        /// <param name="recipe">Recipe to delete</param>
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
        /// Generate a random discount coupon when the user shakes the device.
        /// Saves the coupon to the SQLite database and shows it via DisplayAlert.
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
        /// Use the barometer pressure reading to recommend a recipe.
        /// Low pressure (below 1013 hPa) suggests warm food/drinks (comfort food).
        /// High pressure suggests cold food/drinks (refreshing options).
        /// Called only once per page visit via _barometerInitialized guard.
        /// </summary>
        /// <param name="pressure">Atmospheric pressure in hPa from the barometer sensor</param>
        [RelayCommand]
        public async Task UpdateBarometerRecommendationAsync(double pressure)
        {
            if (_barometerInitialized) return;
            try
            {
                CurrentPressure = pressure;
                _barometerInitialized = true;
                var allRecipes = await _databaseService.GetAllRecipesAsync();
                if (allRecipes.Count == 0) return;

                // Low pressure -> warm/comforting food; high pressure -> cold/refreshing
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

        /// <summary>
        /// Trigger the compass Surprise Me wheel.
        /// Spins for 3 seconds using live compass heading data to drive the animation,
        /// then selects a recipe based on the final heading position.
        /// </summary>
        [RelayCommand]
        public async Task SurpriseMeAsync()
        {
            try
            {
                var allRecipes = await _databaseService.GetAllRecipesAsync();
                if (allRecipes.Count == 0)
                {
                    await Shell.Current.DisplayAlert("No Recipes", "Add some recipes first!", "OK");
                    return;
                }
                IsSpinning = true;
                await Task.Delay(3000);
                IsSpinning = false;

                // Use compass heading modulo count to pick recipe
                int index = (int)(CompassHeading % allRecipes.Count);
                SurpriseRecipe = allRecipes[index];
                ShowSurpriseResult = true;
                bool viewRecipe = await Shell.Current.DisplayAlert("Surprise!",
                    $"The compass chose:\n\n{SurpriseRecipe.Name}\n({SurpriseRecipe.SubCategory})\n\nWould you like to view it?",
                    "View Recipe", "Close");
                ShowSurpriseResult = false;
                if (viewRecipe)
                {
                    await GoToDetailAsync(SurpriseRecipe);
                }
            }
            catch (Exception ex)
            {
                IsSpinning = false;
                await Shell.Current.DisplayAlert("Error", "Compass is not available on this device.", "OK");
                System.Diagnostics.Debug.WriteLine($"SurpriseMe error: {ex.Message}");
            }
        }
    }
}