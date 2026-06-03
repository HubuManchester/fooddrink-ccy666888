using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using TasteHub.Models;
using TasteHub.Services;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Add/Edit Recipe page.
    /// Handles form state, input validation, camera photo capture,
    /// AI food recognition (LogMeal + TheMealDB) and SQLite persistence.
    /// The same view model is reused for both adding new recipes and editing
    /// existing ones, controlled by the IsEditMode flag — code reusability evidence.
    /// </summary>
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class AddEditViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        // ==================== Navigation ====================

        /// <summary>Recipe ID received via Shell navigation query parameter; 0 means Add mode</summary>
        [ObservableProperty]
        private int _recipeId;

        /// <summary>Whether the form is in Edit mode (true) or Add mode (false)</summary>
        [ObservableProperty]
        private bool _isEditMode;

        // ==================== Form Fields ====================

        /// <summary>Recipe name entered by the user (required, max 100 characters)</summary>
        [ObservableProperty]
        private string _recipeName = string.Empty;

        /// <summary>Selected main category: Food or Drink</summary>
        [ObservableProperty]
        private string _selectedCategory = "Food";

        /// <summary>Selected sub-category (e.g. Breakfast, Lunch, Hot Drink)</summary>
        [ObservableProperty]
        private string _selectedSubCategory = string.Empty;

        /// <summary>Optional recipe description text</summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>Local file path or URL for the cover image</summary>
        [ObservableProperty]
        private string _imagePath = string.Empty;

        /// <summary>Calories input as string for Entry binding (validated on save)</summary>
        [ObservableProperty]
        private string _caloriesText = string.Empty;

        /// <summary>Protein input as string for Entry binding (validated on save)</summary>
        [ObservableProperty]
        private string _proteinText = string.Empty;

        /// <summary>Carbohydrates input as string for Entry binding (validated on save)</summary>
        [ObservableProperty]
        private string _carbsText = string.Empty;

        /// <summary>Fat input as string for Entry binding (validated on save)</summary>
        [ObservableProperty]
        private string _fatText = string.Empty;

        // ==================== Validation Error Messages ====================

        /// <summary>Validation error message for the recipe name field</summary>
        [ObservableProperty]
        private string _nameError = string.Empty;

        /// <summary>Validation error message for the category field</summary>
        [ObservableProperty]
        private string _categoryError = string.Empty;

        /// <summary>Validation error message for the nutrition fields</summary>
        [ObservableProperty]
        private string _nutritionError = string.Empty;

        /// <summary>Validation error message for the ingredients list</summary>
        [ObservableProperty]
        private string _ingredientsError = string.Empty;

        /// <summary>Validation error message for the steps list</summary>
        [ObservableProperty]
        private string _stepsError = string.Empty;

        // ==================== Collections ====================

        /// <summary>List of ingredient strings bound to the ingredients CollectionView</summary>
        public ObservableCollection<string> Ingredients { get; } = new();

        /// <summary>List of cooking step strings bound to the steps CollectionView</summary>
        public ObservableCollection<string> Steps { get; } = new();

        /// <summary>Available main categories: Food and Drink</summary>
        public ObservableCollection<string> Categories { get; } = new() { "Food", "Drink" };

        /// <summary>Sub-categories populated dynamically based on selected category</summary>
        public ObservableCollection<string> SubCategories { get; } = new();

        /// <summary>Current text in the ingredient input field</summary>
        [ObservableProperty]
        private string _newIngredient = string.Empty;

        /// <summary>Current text in the step input field</summary>
        [ObservableProperty]
        private string _newStep = string.Empty;

        /// <summary>
        /// Constructor with dependency injection of database service.
        /// Initialises sub-categories for the default Food category.
        /// </summary>
        /// <param name="databaseService">Injected database service for CRUD operations</param>
        public AddEditViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Add Recipe";
            UpdateSubCategories();
        }

        /// <summary>
        /// Switch to Edit mode and load existing recipe data when RecipeId is set via navigation
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
        /// Repopulate sub-categories whenever the main category changes
        /// </summary>
        partial void OnSelectedCategoryChanged(string value)
        {
            UpdateSubCategories();
        }

        /// <summary>
        /// Populate SubCategories based on the selected main category.
        /// Food: Breakfast, Lunch, Dinner, Dessert, Snack.
        /// Drink: Hot Drink, Cold Drink, Smoothie, Juice.
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
        /// Load an existing recipe from the database into the form fields for editing.
        /// Deserialises the JSON-stored ingredients and steps arrays.
        /// </summary>
        private async Task LoadRecipeAsync()
        {
            try
            {
                IsBusy = true;
                var recipe = await _databaseService.GetRecipeByIdAsync(RecipeId);
                if (recipe == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Recipe not found.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Populate all form fields from the loaded recipe
                RecipeName = recipe.Name;
                SelectedCategory = recipe.Category;
                SelectedSubCategory = recipe.SubCategory;
                Description = recipe.Description;
                ImagePath = recipe.ImagePath;
                CaloriesText = recipe.Calories.ToString();
                ProteinText = recipe.Protein.ToString();
                CarbsText = recipe.Carbs.ToString();
                FatText = recipe.Fat.ToString();

                // Deserialise ingredients from JSON string
                Ingredients.Clear();
                try
                {
                    var ingredients = JsonSerializer.Deserialize<string[]>(recipe.Ingredients);
                    if (ingredients != null)
                        foreach (var item in ingredients)
                            Ingredients.Add(item);
                }
                catch (JsonException) { }

                // Deserialise steps from JSON string
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
                await Shell.Current.DisplayAlert("Error", "Failed to load recipe. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"LoadRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==================== Ingredients Commands ====================

        /// <summary>
        /// Add the current NewIngredient text to the ingredients list.
        /// Validates that the input is not empty before adding.
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

        /// <summary>Remove a specific ingredient from the list</summary>
        /// <param name="ingredient">Ingredient string to remove</param>
        [RelayCommand]
        public void RemoveIngredient(string ingredient) => Ingredients.Remove(ingredient);

        // ==================== Steps Commands ====================

        /// <summary>
        /// Add the current NewStep text to the steps list.
        /// Validates that the input is not empty before adding.
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

        /// <summary>Remove a specific step from the list</summary>
        /// <param name="step">Step string to remove</param>
        [RelayCommand]
        public void RemoveStep(string step) => Steps.Remove(step);

        // ==================== Camera Commands ====================

        /// <summary>
        /// Capture a photo using the native Android camera intent.
        /// Requests camera permission at runtime before launching the camera.
        /// Uses CameraService for HarmonyOS-compatible intent handling.
        /// </summary>
        [RelayCommand]
        public async Task TakePhotoAsync()
        {
            try
            {
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    if (cameraStatus != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlert("Permission Denied",
                            "Camera permission is required to take photos.", "OK");
                        return;
                    }
                }

#if ANDROID
                try
                {
                    var photoTask = CameraService.WaitForPhotoAsync();
                    var intent = new Android.Content.Intent(Android.Provider.MediaStore.ActionImageCapture);
                    Platform.CurrentActivity.StartActivityForResult(intent, 1002);
                    string photoPath = await photoTask;
                    if (!string.IsNullOrEmpty(photoPath))
                    {
                        ImagePath = photoPath;
                    }
                }
                catch (Exception camEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Camera intent error: {camEx.Message}");
                    await Shell.Current.DisplayAlert("Camera Error",
                        "Please use Pick from Gallery instead.", "OK");
                }
#else
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                    using var stream = await photo.OpenReadAsync();
                    using var newStream = File.OpenWrite(localPath);
                    await stream.CopyToAsync(newStream);
                    ImagePath = localPath;
                }
#endif
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to take photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"TakePhoto error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pick an existing photo from the device gallery.
        /// Copies the selected photo to the app's local data directory.
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
                await Shell.Current.DisplayAlert("Error", "Failed to pick photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"PickPhoto error: {ex.Message}");
            }
        }

        // ==================== AI Food Recognition ====================

        /// <summary>
        /// Scan the current cover image using the LogMeal deep learning API
        /// to identify the food and auto-fill all recipe form fields.
        /// Pipeline: Image -> LogMeal CNN (dish name + nutrition) -> TheMealDB (description, ingredients, steps).
        /// Requires a cover image to be selected first.
        /// Shows a specific error message for network failures vs unrecognised food.
        /// </summary>
        [RelayCommand]
        public async Task ScanFoodAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(ImagePath))
                {
                    await Shell.Current.DisplayAlert("No Image",
                        "Please take a photo or pick from gallery first.", "OK");
                    return;
                }

                IsBusy = true;

                // Call the dual-API food recognition service
                var service = new FoodRecognitionService();
                var result = await service.RecogniseFoodAsync(ImagePath);

                if (result != null && result.Success)
                {
                    // Auto-fill all form fields with AI-recognised data
                    if (!string.IsNullOrEmpty(result.Name)) RecipeName = result.Name;
                    if (!string.IsNullOrEmpty(result.Description)) Description = result.Description;
                    if (!string.IsNullOrEmpty(result.Category)) SelectedCategory = result.Category;
                    if (!string.IsNullOrEmpty(result.SubCategory)) SelectedSubCategory = result.SubCategory;
                    if (result.Calories > 0) CaloriesText = result.Calories.ToString("F0");
                    if (result.Protein > 0) ProteinText = result.Protein.ToString("F1");
                    if (result.Carbs > 0) CarbsText = result.Carbs.ToString("F1");
                    if (result.Fat > 0) FatText = result.Fat.ToString("F1");

                    if (result.Ingredients != null && result.Ingredients.Length > 0)
                    {
                        Ingredients.Clear();
                        foreach (var ing in result.Ingredients)
                            Ingredients.Add(ing);
                    }

                    if (result.Steps != null && result.Steps.Length > 0)
                    {
                        Steps.Clear();
                        foreach (var step in result.Steps)
                            Steps.Add(step);
                    }

                    await Shell.Current.DisplayAlert("AI Recognition",
                        $"Recognised: {result.Name}\nFields have been auto-filled!", "OK");
                }
                else if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    // Show specific error message (e.g. network error, timeout)
                    await Shell.Current.DisplayAlert("Recognition Failed",
                        $"{result.ErrorMessage}\nPlease fill in the details manually.", "OK");
                }
                else
                {
                    // Food was not recognised by the AI model
                    await Shell.Current.DisplayAlert("AI Recognition",
                        "Could not recognise the food. Please fill in manually.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Food recognition failed. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"ScanFood error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==================== Validation ====================

        /// <summary>
        /// Validate all form fields before saving.
        /// Checks: name (required, max 100 chars), category (required),
        /// nutrition (numeric, within bounds), ingredients (min 1), steps (min 1).
        /// Sets error message properties for display in the UI (WCAG 3.3.1).
        /// </summary>
        /// <returns>True if all fields are valid; false otherwise</returns>
        private bool ValidateForm()
        {
            bool isValid = true;

            // Validate recipe name
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

            // Validate category selection
            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                CategoryError = "Please select a category.";
                isValid = false;
            }
            else
            {
                CategoryError = string.Empty;
            }

            // Validate nutrition fields: must be numeric and within realistic bounds
            NutritionError = string.Empty;
            if (!string.IsNullOrWhiteSpace(CaloriesText))
            {
                if (!double.TryParse(CaloriesText, out double cal) || cal < 0 || cal > 10000)
                {
                    NutritionError = "Calories must be a number between 0 and 10000.";
                    isValid = false;
                }
            }
            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(ProteinText))
            {
                if (!double.TryParse(ProteinText, out double pro) || pro < 0 || pro > 1000)
                {
                    NutritionError = "Protein must be a number between 0 and 1000.";
                    isValid = false;
                }
            }
            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(CarbsText))
            {
                if (!double.TryParse(CarbsText, out double carb) || carb < 0 || carb > 1000)
                {
                    NutritionError = "Carbs must be a number between 0 and 1000.";
                    isValid = false;
                }
            }
            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(FatText))
            {
                if (!double.TryParse(FatText, out double fat) || fat < 0 || fat > 1000)
                {
                    NutritionError = "Fat must be a number between 0 and 1000.";
                    isValid = false;
                }
            }

            // Validate at least one ingredient
            if (Ingredients.Count == 0)
            {
                IngredientsError = "Please add at least one ingredient.";
                isValid = false;
            }
            else
            {
                IngredientsError = string.Empty;
            }

            // Validate at least one cooking step
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

        // ==================== Save Command ====================

        /// <summary>
        /// Validate and save the recipe to the SQLite database.
        /// Serialises ingredients and steps to JSON for storage.
        /// Clears the form after a successful save to prepare for the next entry.
        /// Works for both Add (Id=0) and Edit (Id>0) modes via DatabaseService.SaveRecipeAsync.
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

                // Clear form after save to prepare for the next entry
                RecipeName = string.Empty;
                Description = string.Empty;
                SelectedCategory = "Food";
                SelectedSubCategory = null;
                ImagePath = string.Empty;
                CaloriesText = string.Empty;
                ProteinText = string.Empty;
                CarbsText = string.Empty;
                FatText = string.Empty;
                Ingredients.Clear();
                Steps.Clear();
                NewIngredient = string.Empty;
                NewStep = string.Empty;
                NameError = string.Empty;
                CategoryError = string.Empty;
                NutritionError = string.Empty;
                IngredientsError = string.Empty;
                StepsError = string.Empty;
                IsEditMode = false;
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