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
    /// View model for the Add/Edit page, handling recipe creation and modification
    /// with camera integration and AI food recognition
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

        [ObservableProperty]
        private string _recipeName = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "Food";

        [ObservableProperty]
        private string _selectedSubCategory = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private string _caloriesText = string.Empty;

        [ObservableProperty]
        private string _proteinText = string.Empty;

        [ObservableProperty]
        private string _carbsText = string.Empty;

        [ObservableProperty]
        private string _fatText = string.Empty;

        // ==================== Validation Error Messages ====================

        [ObservableProperty]
        private string _nameError = string.Empty;

        [ObservableProperty]
        private string _categoryError = string.Empty;

        [ObservableProperty]
        private string _nutritionError = string.Empty;

        [ObservableProperty]
        private string _ingredientsError = string.Empty;

        [ObservableProperty]
        private string _stepsError = string.Empty;

        // ==================== Collections ====================

        public ObservableCollection<string> Ingredients { get; } = new();
        public ObservableCollection<string> Steps { get; } = new();
        public ObservableCollection<string> Categories { get; } = new() { "Food", "Drink" };
        public ObservableCollection<string> SubCategories { get; } = new();

        [ObservableProperty]
        private string _newIngredient = string.Empty;

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

        partial void OnRecipeIdChanged(int value)
        {
            if (value != 0)
            {
                IsEditMode = true;
                Title = "Edit Recipe";
                MainThread.BeginInvokeOnMainThread(async () => await LoadRecipeAsync());
            }
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            UpdateSubCategories();
        }

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

                RecipeName = recipe.Name;
                SelectedCategory = recipe.Category;
                SelectedSubCategory = recipe.SubCategory;
                Description = recipe.Description;
                ImagePath = recipe.ImagePath;
                CaloriesText = recipe.Calories.ToString();
                ProteinText = recipe.Protein.ToString();
                CarbsText = recipe.Carbs.ToString();
                FatText = recipe.Fat.ToString();

                Ingredients.Clear();
                try
                {
                    var ingredients = JsonSerializer.Deserialize<string[]>(recipe.Ingredients);
                    if (ingredients != null)
                        foreach (var item in ingredients)
                            Ingredients.Add(item);
                }
                catch (JsonException) { }

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
                await Shell.Current.DisplayAlert("Error", "Failed to load recipe.", "OK");
                System.Diagnostics.Debug.WriteLine($"LoadRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

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

        [RelayCommand]
        public void RemoveIngredient(string ingredient)
        {
            Ingredients.Remove(ingredient);
        }

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

        [RelayCommand]
        public void RemoveStep(string step)
        {
            Steps.Remove(step);
        }

        /// <summary>
        /// Take a photo using camera with HarmonyOS compatibility.
        /// Uses CameraService to handle native Android camera intent result.
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
                    var intent = new Android.Content.Intent(
                        Android.Provider.MediaStore.ActionImageCapture);
                    Platform.CurrentActivity.StartActivityForResult(intent, 1002);
                    string photoPath = await photoTask;

                    if (!string.IsNullOrEmpty(photoPath))
                    {
                        ImagePath = photoPath;
                    }
                }
                catch (Exception camEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Camera error: {camEx.Message}");
                    await Shell.Current.DisplayAlert("Camera Error",
                        "Please use Pick from Gallery instead.", "OK");
                }
#else
                try
                {
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
                catch
                {
                    await Shell.Current.DisplayAlert("Camera Unavailable",
                        "Please use Pick from Gallery.", "OK");
                }
#endif
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to access camera.", "OK");
                System.Diagnostics.Debug.WriteLine($"TakePhoto error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pick a photo from the device gallery
        /// </summary>
        [RelayCommand]
        public async Task PickPhotoAsync()
        {
            try
            {
                var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (storageStatus != PermissionStatus.Granted)
                    storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

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
                    "Failed to pick photo.", "OK");
                System.Diagnostics.Debug.WriteLine($"PickPhoto error: {ex.Message}");
            }
        }

        /// <summary>
        /// Scan food using camera and AI recognition.
        /// 1. Takes/picks a photo
        /// 2. Sends to LogMeal deep learning API for dish identification
        /// 3. Fetches nutritional info from LogMeal AI
        /// 4. Fetches recipe details from TheMealDB API
        /// 5. Auto-fills ALL form fields: name, category, description,
        ///    nutrition, ingredients and steps
        /// </summary>
        [RelayCommand]
        public async Task ScanFoodAsync()
        {
            try
            {
                // Request camera permission
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    if (cameraStatus != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlert("Permission Denied",
                            "Camera permission is required to scan food.", "OK");
                        return;
                    }
                }

                // Get image: try camera first, then gallery
                string imagePath = null;

#if ANDROID
                try
                {
                    var photoTask = CameraService.WaitForPhotoAsync();
                    var intent = new Android.Content.Intent(
                        Android.Provider.MediaStore.ActionImageCapture);
                    Platform.CurrentActivity.StartActivityForResult(intent, 1002);
                    imagePath = await photoTask;
                }
                catch
                {
                    // Camera failed, will try gallery below
                }
#endif

                // Fallback to gallery
                if (string.IsNullOrEmpty(imagePath))
                {
                    try
                    {
                        var photo = await MediaPicker.Default.PickPhotoAsync();
                        if (photo != null)
                        {
                            imagePath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                            using var stream = await photo.OpenReadAsync();
                            using var newStream = File.OpenWrite(imagePath);
                            await stream.CopyToAsync(newStream);
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    await Shell.Current.DisplayAlert("No Image",
                        "Please take or select a food photo.", "OK");
                    return;
                }

                // Set image preview
                ImagePath = imagePath;

                // Show loading message
                IsBusy = true;
                await Shell.Current.DisplayAlert("Scanning...",
                    "Analysing food image with AI.\nThis may take a few seconds.", "OK");

                // Call food recognition service (LogMeal + TheMealDB)
                var recognitionService = new FoodRecognitionService();
                var result = await recognitionService.RecogniseFoodAsync(imagePath);

                IsBusy = false;

                if (result.Success)
                {
                    // ===== Auto-fill name =====
                    RecipeName = result.Name;

                    // ===== Auto-fill category =====
                    SelectedCategory = result.Category;
                    // Wait for SubCategories to update
                    await Task.Delay(100);
                    if (!string.IsNullOrEmpty(result.SubCategory))
                    {
                        SelectedSubCategory = result.SubCategory;
                    }

                    // ===== Auto-fill description =====
                    if (!string.IsNullOrEmpty(result.Description))
                    {
                        Description = result.Description;
                    }

                    // ===== Auto-fill nutrition =====
                    CaloriesText = result.Calories > 0 ? result.Calories.ToString("F0") : "";
                    ProteinText = result.Protein > 0 ? result.Protein.ToString("F1") : "";
                    CarbsText = result.Carbs > 0 ? result.Carbs.ToString("F1") : "";
                    FatText = result.Fat > 0 ? result.Fat.ToString("F1") : "";

                    // ===== Auto-fill ingredients =====
                    Ingredients.Clear();
                    if (result.Ingredients != null && result.Ingredients.Length > 0)
                    {
                        foreach (var ingredient in result.Ingredients)
                        {
                            if (!string.IsNullOrWhiteSpace(ingredient))
                            {
                                Ingredients.Add(ingredient);
                            }
                        }
                    }
                    IngredientsError = string.Empty;

                    // ===== Auto-fill steps =====
                    Steps.Clear();
                    if (result.Steps != null && result.Steps.Length > 0)
                    {
                        foreach (var step in result.Steps)
                        {
                            if (!string.IsNullOrWhiteSpace(step))
                            {
                                Steps.Add(step);
                            }
                        }
                    }
                    StepsError = string.Empty;

                    // Show summary
                    int ingredientCount = Ingredients.Count;
                    int stepCount = Steps.Count;

                    string nutritionInfo = result.Calories > 0
                        ? $"\nCalories: {result.Calories:F0} kcal" +
                          $"\nProtein: {result.Protein:F1}g" +
                          $"\nCarbs: {result.Carbs:F1}g" +
                          $"\nFat: {result.Fat:F1}g"
                        : "\nNutritional info not available.";

                    string recipeInfo = ingredientCount > 0
                        ? $"\nIngredients: {ingredientCount} items" +
                          $"\nSteps: {stepCount} steps"
                        : "\nRecipe details not found in database.";

                    await Shell.Current.DisplayAlert("Food Recognised!",
                        $"Detected: {result.Name}" +
                        $"\nCategory: {result.Category} > {result.SubCategory}" +
                        nutritionInfo +
                        recipeInfo,
                        "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Recognition Failed",
                        $"{result.ErrorMessage}\n\nPlease fill in the details manually.", "OK");
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Shell.Current.DisplayAlert("Error",
                    "Food scanning failed. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"ScanFood error: {ex.Message}");
            }
        }

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

            // Validate nutrition fields (must be valid, non-negative numbers within range)
            NutritionError = string.Empty;
            if (!string.IsNullOrWhiteSpace(CaloriesText))
            {
                if (!double.TryParse(CaloriesText, out double calVal))
                {
                    NutritionError = "Calories must be a valid number.";
                    isValid = false;
                }
                else if (calVal < 0 || calVal > 10000)
                {
                    NutritionError = "Calories must be between 0 and 10000.";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(ProteinText))
            {
                if (!double.TryParse(ProteinText, out double proVal))
                {
                    NutritionError = "Protein must be a valid number.";
                    isValid = false;
                }
                else if (proVal < 0 || proVal > 1000)
                {
                    NutritionError = "Protein must be between 0 and 1000g.";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(CarbsText))
            {
                if (!double.TryParse(CarbsText, out double carbVal))
                {
                    NutritionError = "Carbs must be a valid number.";
                    isValid = false;
                }
                else if (carbVal < 0 || carbVal > 1000)
                {
                    NutritionError = "Carbs must be between 0 and 1000g.";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(NutritionError) && !string.IsNullOrWhiteSpace(FatText))
            {
                if (!double.TryParse(FatText, out double fatVal))
                {
                    NutritionError = "Fat must be a valid number.";
                    isValid = false;
                }
                else if (fatVal < 0 || fatVal > 1000)
                {
                    NutritionError = "Fat must be between 0 and 1000g.";
                    isValid = false;
                }
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
                    "Failed to save recipe.", "OK");
                System.Diagnostics.Debug.WriteLine($"SaveRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}