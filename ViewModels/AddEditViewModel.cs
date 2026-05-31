using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using TasteHub.Models;
using TasteHub.Services;
using static Android.Icu.Text.CaseMap;
using static Android.Util.EventLogTags;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Add/Edit page, handling recipe creation and modification
    /// with camera integration for cover photos
    /// </summary>
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class AddEditViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        /// <summary>Recipe ID for edit mode; 0 means adding a new recipe</summary>
        [ObservableProperty]
        private int _recipeId;

        /// <summary>Whether the form is in edit mode</summary>
        [ObservableProperty]
        private bool _isEditMode;

        // ==================== Form Fields ====================

        /// <summary>Recipe name input</summary>
        [ObservableProperty]
        private string _recipeName = string.Empty;

        /// <summary>Selected main category: Food or Drink</summary>
        [ObservableProperty]
        private string _selectedCategory = "Food";

        /// <summary>Selected sub-category</summary>
        [ObservableProperty]
        private string _selectedSubCategory = string.Empty;

        /// <summary>Recipe description input</summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>Cover image file path</summary>
        [ObservableProperty]
        private string _imagePath = string.Empty;

        /// <summary>Calories input</summary>
        [ObservableProperty]
        private string _caloriesText = string.Empty;

        /// <summary>Protein input</summary>
        [ObservableProperty]
        private string _proteinText = string.Empty;

        /// <summary>Carbohydrates input</summary>
        [ObservableProperty]
        private string _carbsText = string.Empty;

        /// <summary>Fat input</summary>
        [ObservableProperty]
        private string _fatText = string.Empty;

        // ==================== Validation Error Messages ====================

        /// <summary>Validation error for recipe name</summary>
        [ObservableProperty]
        private string _nameError = string.Empty;

        /// <summary>Validation error for category</summary>
        [ObservableProperty]
        private string _categoryError = string.Empty;

        /// <summary>Validation error for nutrition fields</summary>
        [ObservableProperty]
        private string _nutritionError = string.Empty;

        /// <summary>Validation error for ingredients</summary>
        [ObservableProperty]
        private string _ingredientsError = string.Empty;

        /// <summary>Validation error for steps</summary>
        [ObservableProperty]
        private string _stepsError = string.Empty;

        // ==================== Collections ====================

        /// <summary>List of ingredients the user has added</summary>
        public ObservableCollection<string> Ingredients { get; } = new();

        /// <summary>List of cooking steps the user has added</summary>
        public ObservableCollection<string> Steps { get; } = new();

        /// <summary>Available main categories</summary>
        public ObservableCollection<string> Categories { get; } = new() { "Food", "Drink" };

        /// <summary>Available sub-categories, changes based on selected main category</summary>
        public ObservableCollection<string> SubCategories { get; } = new();

        /// <summary>New ingredient text input</summary>
        [ObservableProperty]
        private string _newIngredient = string.Empty;

        /// <summary>New step text input</summary>
        [ObservableProperty]
        private string _newStep = string.Empty;

        /// <summary>
        /// Constructor with dependency injection of database service
        /// </summary>
        public AddEditViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Add Recipe";
            UpdateSubCategories();
        }

        /// <summary>
        /// Load existing recipe data when RecipeId is set (edit mode)
        /// </summary>
        partial void OnRecipeIdChanged(int value)
        {
            if (value != 0)
            {
                IsEditMode = true;
                Title = "Edit Recipe";
                MainThread.BeginInvokeOnMainThread(async () => await LoadRecipeAsync());
            }
        }

        /// <summary>
        /// Update sub-category options when main category changes
        /// </summary>
        partial void OnSelectedCategoryChanged(string value)
        {
            UpdateSubCategories();
        }

        /// <summary>
        /// Populate sub-category list based on the selected main category
        /// </summary>
        private void UpdateSubCategories()
        {
            SubCategories.Clear();
            if (SelectedCategory == "Food")
            {
                SubCategories.Add("Breakfast");
                SubCategories.Add("Lunch");
                SubCategories.Add("Dinner");
                SubCategories.Add("Dessert");
                SubCategories.Add("Snack");
            }
            else
            {
                SubCategories.Add("Hot Drink");
                SubCategories.Add("Cold Drink");
                SubCategories.Add("Smoothie");
                SubCategories.Add("Juice");
            }

            if (SubCategories.Count > 0 && string.IsNullOrEmpty(SelectedSubCategory))
            {
                SelectedSubCategory = SubCategories[0];
            }
        }

        /// <summary>
        /// Load recipe data from database for editing
        /// </summary>
        private async Task LoadRecipeAsync()
        {
            try
            {
                IsBusy = true;
                var recipe = await _databaseService.GetRecipeByIdAsync(RecipeId);

                if (recipe == null)
                {
                    await Shell.Current.DisplayAlert("Error",
                        "Recipe not found.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                RecipeName = recipe.Name;
                SelectedCategory = recipe.Category;
                SelectedSubCategory = recipe.SubCategory;
                Description = recipe.Description;
                ImagePath = recipe.ImagePath;
                CaloriesText = recipe.Calories.ToString();
                ProteinText = recipe.Protein.ToString();
                CarbsText = recipe.Carbs.ToString();
                FatText = recipe.Fat.ToString();

                // Parse ingredients
                Ingredients.Clear();
                try
                {
                    var ingredients = JsonSerializer.Deserialize<string[]>(recipe.Ingredients);
                    if (ingredients != null)
                        foreach (var item in ingredients)
                            Ingredients.Add(item);
                }
                catch (JsonException) { }

                // Parse steps
                Steps.Clear();
                try
                {
                    var steps = JsonSerializer.Deserialize<string[]>(recipe.Steps);
                    if (steps != null)
                        foreach (var step in steps)
                            Steps.Add(step);
                }
                catch (JsonException) { }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to load recipe. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"LoadRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Add a new ingredient to the list
        /// </summary>
        [RelayCommand]
        public void AddIngredient()
        {
            if (string.IsNullOrWhiteSpace(NewIngredient))
            {
                IngredientsError = "Please enter an ingredient.";
                return;
            }

            Ingredients.Add(NewIngredient.Trim());
            NewIngredient = string.Empty;
            IngredientsError = string.Empty;
        }

        /// <summary>
        /// Remove an ingredient from the list
        /// </summary>
        [RelayCommand]
        public void RemoveIngredient(string ingredient)
        {
            Ingredients.Remove(ingredient);
        }

        /// <summary>
        /// Add a new cooking step to the list
        /// </summary>
        [RelayCommand]
        public void AddStep()
        {
            if (string.IsNullOrWhiteSpace(NewStep))
            {
                StepsError = "Please enter a step.";
                return;
            }

            Steps.Add(NewStep.Trim());
            NewStep = string.Empty;
            StepsError = string.Empty;
        }

        /// <summary>
        /// Remove a cooking step from the list
        /// </summary>
        [RelayCommand]
        public void RemoveStep(string step)
        {
            Steps.Remove(step);
        }

        /// <summary>
        /// Take a photo using the device camera for the recipe cover image
        /// </summary>
        [RelayCommand]
        public async Task TakePhotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert("Error",
                        "Camera is not supported on this device.", "OK");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                    using var stream = await photo.OpenReadAsync();
                    using var newStream = File.OpenWrite(localPath);
                    await stream.CopyToAsync(newStream);
                    ImagePath = localPath;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to take photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"TakePhoto error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pick an existing photo from the device gallery
        /// </summary>
        [RelayCommand]
        public async Task PickPhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                    using var stream = await photo.OpenReadAsync();
                    using var newStream = File.OpenWrite(localPath);
                    await stream.CopyToAsync(newStream);
                    ImagePath = localPath;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to pick photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"PickPhoto error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate all form fields and return true if valid
        /// </summary>
        private bool ValidateForm()
        {
            bool isValid = true;

            // Validate name
            if (string.IsNullOrWhiteSpace(RecipeName))
            {
                NameError = "Recipe name is required.";
                isValid = false;
            }
            else if (RecipeName.Length > 100)
            {
                NameError = "Recipe name must be under 100 characters.";
                isValid = false;
            }
            else
            {
                NameError = string.Empty;
            }

            // Validate category
            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                CategoryError = "Please select a category.";
                isValid = false;
            }
            else
            {
                CategoryError = string.Empty;
            }

            // Validate nutrition (must be non-negative numbers)
            NutritionError = string.Empty;
            if (!string.IsNullOrWhiteSpace(CaloriesText) && !double.TryParse(CaloriesText, out double cal))
            {
                NutritionError = "Calories must be a valid number.";
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(ProteinText) && !double.TryParse(ProteinText, out double pro))
            {
                NutritionError = "Protein must be a valid number.";
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(CarbsText) && !double.TryParse(CarbsText, out double carb))
            {
                NutritionError = "Carbs must be a valid number.";
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(FatText) && !double.TryParse(FatText, out double fat))
            {
                NutritionError = "Fat must be a valid number.";
                isValid = false;
            }

            // Validate ingredients
            if (Ingredients.Count == 0)
            {
                IngredientsError = "Please add at least one ingredient.";
                isValid = false;
            }
            else
            {
                IngredientsError = string.Empty;
            }

            // Validate steps
            if (Steps.Count == 0)
            {
                StepsError = "Please add at least one step.";
                isValid = false;
            }
            else
            {
                StepsError = string.Empty;
            }

            return isValid;
        }

        /// <summary>
        /// Save the recipe to the database after validation
        /// </summary>
        [RelayCommand]
        public async Task SaveRecipeAsync()
        {
            if (!ValidateForm()) return;

            try
            {
                IsBusy = true;

                var recipe = new Recipe
                {
                    Id = IsEditMode ? RecipeId : 0,
                    Name = RecipeName.Trim(),
                    Category = SelectedCategory,
                    SubCategory = SelectedSubCategory,
                    Description = Description?.Trim() ?? string.Empty,
                    ImagePath = ImagePath ?? string.Empty,
                    Ingredients = JsonSerializer.Serialize(Ingredients),
                    Steps = JsonSerializer.Serialize(Steps),
                    Calories = double.TryParse(CaloriesText, out double c) ? c : 0,
                    Protein = double.TryParse(ProteinText, out double p) ? p : 0,
                    Carbs = double.TryParse(CarbsText, out double cb) ? cb : 0,
                    Fat = double.TryParse(FatText, out double f) ? f : 0
                };

                await _databaseService.SaveRecipeAsync(recipe);

                await Shell.Current.DisplayAlert("Success",
                    IsEditMode ? "Recipe updated successfully!" : "Recipe added successfully!", "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to save recipe. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"SaveRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}