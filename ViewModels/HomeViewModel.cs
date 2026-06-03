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
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private bool _isLoading = false;
        private bool _barometerInitialized = false;

        public ObservableCollection<Recipe> Recipes { get; } = new();

        [ObservableProperty]
        private string _selectedCategory = "All";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private Recipe _recommendedRecipe;

        [ObservableProperty]
        private bool _showCouponPopup;

        [ObservableProperty]
        private Coupon _latestCoupon;

        [ObservableProperty]
        private double _currentPressure;

        [ObservableProperty]
        private double _compassHeading;

        [ObservableProperty]
        private bool _isSpinning;

        [ObservableProperty]
        private Recipe _surpriseRecipe;

        [ObservableProperty]
        private bool _showSurpriseResult;

        public HomeViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "TasteHub";
        }

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

                if (SelectedCategory != "All")
                {
                    recipes = recipes.Where(r => r.Category == SelectedCategory).ToList();
                }

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

        partial void OnSearchTextChanged(string value)
        {
            LoadRecipesAsync().ConfigureAwait(false);
        }

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