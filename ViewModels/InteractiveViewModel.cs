using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TasteHub.Models;
using static Android.Icu.Text.CaseMap;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Interactive cooking page, handling accelerometer,
    /// gyroscope and shake hardware features for simulated cooking experience
    /// </summary>
    [QueryProperty(nameof(RecipeName), "name")]
    [QueryProperty(nameof(RecipeCategory), "category")]
    public partial class InteractiveViewModel : BaseViewModel
    {
        /// <summary>Name of the recipe being prepared</summary>
        [ObservableProperty]
        private string _recipeName = string.Empty;

        /// <summary>Category of the recipe (Food or Drink)</summary>
        [ObservableProperty]
        private string _recipeCategory = string.Empty;

        // ==================== Accelerometer (Tilt to Pour) ====================

        /// <summary>Current tilt angle of the device in degrees</summary>
        [ObservableProperty]
        private double _tiltAngle;

        /// <summary>Pour progress from 0 to 100 based on tilt angle</summary>
        [ObservableProperty]
        private double _pourProgress;

        /// <summary>Whether the pouring step is completed</summary>
        [ObservableProperty]
        private bool _pourCompleted;

        /// <summary>Label describing the pour action based on recipe category</summary>
        [ObservableProperty]
        private string _pourLabel = "Tilt to pour ingredients";

        // ==================== Gyroscope (Rotate to Stir) ====================

        /// <summary>Current rotation speed detected by gyroscope</summary>
        [ObservableProperty]
        private double _rotationSpeed;

        /// <summary>Stir progress from 0 to 100 based on cumulative rotation</summary>
        [ObservableProperty]
        private double _stirProgress;

        /// <summary>Whether the stirring step is completed</summary>
        [ObservableProperty]
        private bool _stirCompleted;

        /// <summary>Visual rotation angle for the stir animation</summary>
        [ObservableProperty]
        private double _stirAnimationAngle;

        // ==================== Shake (Shake to Mix) ====================

        /// <summary>Number of shakes detected</summary>
        [ObservableProperty]
        private int _shakeCount;

        /// <summary>Shake progress from 0 to 100 based on shake count</summary>
        [ObservableProperty]
        private double _shakeProgress;

        /// <summary>Whether the shaking step is completed</summary>
        [ObservableProperty]
        private bool _shakeCompleted;

        // ==================== Overall Progress ====================

        /// <summary>Current step index (0=Pour, 1=Stir, 2=Shake)</summary>
        [ObservableProperty]
        private int _currentStep;

        /// <summary>Whether all cooking steps are completed</summary>
        [ObservableProperty]
        private bool _allCompleted;

        /// <summary>Status message displayed to the user</summary>
        [ObservableProperty]
        private string _statusMessage = "Step 1: Tilt your phone to pour ingredients!";

        /// <summary>
        /// Constructor
        /// </summary>
        public InteractiveViewModel()
        {
            Title = "Interactive Cooking";
        }

        /// <summary>
        /// Update pour label based on recipe category
        /// </summary>
        partial void OnRecipeCategoryChanged(string value)
        {
            PourLabel = value == "Drink"
                ? "Tilt to pour liquid"
                : "Tilt to pour ingredients";
        }

        /// <summary>
        /// Process accelerometer reading to update pour progress.
        /// Maps tilt angle to a 0-100 progress value.
        /// </summary>
        [RelayCommand]
        public void UpdateAccelerometer(double zValue)
        {
            if (PourCompleted) return;

            // Map Z-axis value to tilt angle (0-90 degrees)
            TiltAngle = Math.Abs(zValue) * 90;

            // Only count significant tilts (more than 30 degrees)
            if (TiltAngle > 30)
            {
                double increment = (TiltAngle - 30) / 60 * 2;
                PourProgress = Math.Min(100, PourProgress + increment);
            }

            if (PourProgress >= 100)
            {
                PourCompleted = true;
                CurrentStep = 1;
                StatusMessage = "Step 2: Rotate your phone to stir!";
            }
        }

        /// <summary>
        /// Process gyroscope reading to update stir progress.
        /// Maps rotation speed to stir animation and progress.
        /// </summary>
        [RelayCommand]
        public void UpdateGyroscope(double angularVelocity)
        {
            if (!PourCompleted || StirCompleted) return;

            RotationSpeed = Math.Abs(angularVelocity);

            // Update animation angle
            StirAnimationAngle = (StirAnimationAngle + angularVelocity * 10) % 360;

            // Increment progress based on rotation speed
            if (RotationSpeed > 0.5)
            {
                double increment = RotationSpeed * 0.5;
                StirProgress = Math.Min(100, StirProgress + increment);
            }

            if (StirProgress >= 100)
            {
                StirCompleted = true;
                CurrentStep = 2;
                StatusMessage = "Step 3: Shake your phone to mix!";
            }
        }

        /// <summary>
        /// Increment shake count and update progress when a shake is detected
        /// </summary>
        [RelayCommand]
        public void ShakeDetected()
        {
            if (!StirCompleted || ShakeCompleted) return;

            ShakeCount++;
            ShakeProgress = Math.Min(100, ShakeCount * 10);

            if (ShakeProgress >= 100)
            {
                ShakeCompleted = true;
                AllCompleted = true;
                StatusMessage = "All done! Your dish is ready!";
            }
        }

        /// <summary>
        /// Reset all progress and start the cooking process again
        /// </summary>
        [RelayCommand]
        public void ResetCooking()
        {
            PourProgress = 0;
            PourCompleted = false;
            StirProgress = 0;
            StirCompleted = false;
            StirAnimationAngle = 0;
            ShakeCount = 0;
            ShakeProgress = 0;
            ShakeCompleted = false;
            AllCompleted = false;
            CurrentStep = 0;
            StatusMessage = "Step 1: Tilt your phone to pour ingredients!";
        }
    }
}