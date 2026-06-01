using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TasteHub.Models;
using TasteHub.Services;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Detail page, displaying full recipe information
    /// with text-to-speech step-by-step reading
    /// </summary>
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class DetailViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private CancellationTokenSource _ttsCancellation;

        /// <summary>Recipe ID passed via navigation query</summary>
        [ObservableProperty]
        private int _recipeId;

        /// <summary>The full recipe object loaded from database</summary>
        [ObservableProperty]
        private Recipe _recipe;

        /// <summary>Parsed list of ingredients for display</summary>
        public ObservableCollection<string> IngredientsList { get; } = new();

        /// <summary>Parsed list of cooking steps for display</summary>
        public ObservableCollection<string> StepsList { get; } = new();

        /// <summary>Index of the step currently being read aloud by TTS</summary>
        [ObservableProperty]
        private int _currentReadingStep = -1;

        /// <summary>Whether TTS is currently reading steps</summary>
        [ObservableProperty]
        private bool _isReading;

        /// <summary>Current image scale for pinch-to-zoom</summary>
        [ObservableProperty]
        private double _imageScale = 1.0;

        /// <summary>
        /// Constructor with dependency injection of database service
        /// </summary>
        public DetailViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Recipe Details";
        }

        /// <summary>
        /// Load recipe data when the RecipeId property changes
        /// </summary>
        partial void OnRecipeIdChanged(int value)
        {
            MainThread.BeginInvokeOnMainThread(async () => await LoadRecipeAsync());
        }

        /// <summary>
        /// Load the full recipe from the database and parse ingredients and steps
        /// </summary>
        [RelayCommand]
        public async Task LoadRecipeAsync()
        {
            if (IsBusy) return;

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

                Recipe = recipe;
                Title = recipe.Name;

                // Parse ingredients from JSON string
                IngredientsList.Clear();
                try
                {
                    var ingredients = JsonSerializer.Deserialize<string[]>(recipe.Ingredients);
                    if (ingredients != null)
                    {
                        foreach (var item in ingredients)
                            IngredientsList.Add(item);
                    }
                }
                catch (JsonException)
                {
                    IngredientsList.Add("Unable to load ingredients");
                }

                // Parse steps from JSON string
                StepsList.Clear();
                try
                {
                    var steps = JsonSerializer.Deserialize<string[]>(recipe.Steps);
                    if (steps != null)
                    {
                        foreach (var step in steps)
                            StepsList.Add(step);
                    }
                }
                catch (JsonException)
                {
                    StepsList.Add("Unable to load steps");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to load recipe details. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"LoadRecipe error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Read all cooking steps aloud using text-to-speech,
        /// highlighting the current step visually.
        /// Supports cancellation when user taps Stop.
        /// </summary>
        [RelayCommand]
        public async Task ReadStepsAloudAsync()
        {
            if (IsReading)
            {
                // Stop reading
                IsReading = false;
                CurrentReadingStep = -1;
                _ttsCancellation?.Cancel();
                return;
            }

            try
            {
                IsReading = true;
                _ttsCancellation = new CancellationTokenSource();

                for (int i = 0; i < StepsList.Count; i++)
                {
                    if (!IsReading) break;

                    CurrentReadingStep = i;

                    var settings = new SpeechOptions
                    {
                        Pitch = 1.0f,
                        Volume = 1.0f
                    };

                    await TextToSpeech.Default.SpeakAsync(
                        $"Step {i + 1}: {StepsList[i]}", settings,
                        _ttsCancellation.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // TTS was cancelled by user, this is expected
                System.Diagnostics.Debug.WriteLine("TTS cancelled by user");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Text-to-speech is not available on this device.", "OK");
                System.Diagnostics.Debug.WriteLine($"TTS error: {ex.Message}");
            }
            finally
            {
                IsReading = false;
                CurrentReadingStep = -1;
                _ttsCancellation?.Dispose();
                _ttsCancellation = null;
            }
        }

        /// <summary>
        /// Navigate to the edit page for the current recipe
        /// </summary>
        [RelayCommand]
        public async Task EditRecipeAsync()
        {
            if (Recipe == null) return;

            try
            {
                await Shell.Current.GoToAsync($"AddEditPage?id={Recipe.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to open edit page. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"EditRecipe error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete the current recipe with confirmation
        /// </summary>
        [RelayCommand]
        public async Task DeleteRecipeAsync()
        {
            if (Recipe == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete",
                $"Are you sure you want to delete '{Recipe.Name}'?", "Yes", "No");

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteRecipeAsync(Recipe);
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error",
                        "Failed to delete recipe. Please try again.", "OK");
                    System.Diagnostics.Debug.WriteLine($"DeleteRecipe error: {ex.Message}");
                }
            }
        }
    }
}