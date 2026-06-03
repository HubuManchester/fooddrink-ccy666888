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
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class AddEditViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private int _recipeId;

        [ObservableProperty]
        private bool _isEditMode;

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

        public ObservableCollection<string> Ingredients { get; } = new();
        public ObservableCollection<string> Steps { get; } = new();
        public ObservableCollection<string> Categories { get; } = new() { "Food", "Drink" };
        public ObservableCollection<string> SubCategories { get; } = new();

        [ObservableProperty]
        private string _newIngredient = string.Empty;

        [ObservableProperty]
        private string _newStep = string.Empty;

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
                await Shell.Current.DisplayAlert("Error", "Failed to load recipe. Please try again.", "OK");
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
        public void RemoveIngredient(string ingredient) => Ingredients.Remove(ingredient);

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
        public void RemoveStep(string step) => Steps.Remove(step);

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
                    var activity = Platform.CurrentActivity;
                    if (activity != null)
                    {
                        string fileName = $"TasteHub_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                        Services.CameraService.SetPhotoPath(filePath);

                        var intent = new Android.Content.Intent(Android.Provider.MediaStore.ActionImageCapture);
                        activity.StartActivityForResult(intent, 1002);

                        string resultPath = await Services.CameraService.WaitForPhotoAsync();
                        if (!string.IsNullOrEmpty(resultPath) && File.Exists(resultPath))
                        {
                            ImagePath = resultPath;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Native camera error: {ex.Message}");
                }
#endif

                await Shell.Current.DisplayAlert("No Photo",
                    "No photo was taken. Please try again or use 'Gallery' to pick an existing photo.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to take photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"TakePhoto error: {ex.Message}");
            }
        }

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
                await Shell.Current.DisplayAlert("Error", "Failed to pick photo. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"PickPhoto error: {ex.Message}");
            }
        }

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

                var service = new FoodRecognitionService();
                var result = await service.RecogniseFoodAsync(ImagePath);

                if (result != null)
                {
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
                else
                {
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

        private bool ValidateForm()
        {
            bool isValid = true;

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

            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                CategoryError = "Please select a category.";
                isValid = false;
            }
            else
            {
                CategoryError = string.Empty;
            }

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

            if (Ingredients.Count == 0)
            {
                IngredientsError = "Please add at least one ingredient.";
                isValid = false;
            }
            else
            {
                IngredientsError = string.Empty;
            }

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

                // Clear form after save
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