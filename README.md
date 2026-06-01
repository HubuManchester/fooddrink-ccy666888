# TasteHub - Food and Drink Recipe App

## Author
**Changyu Chen** - Student ID: 20906256

## Module
6G6Z0014 - Mobile Computing | Manchester Metropolitan University

---

## Overview
TasteHub is a cross-platform food and drink recipe management application built with .NET MAUI (.NET 8). Users can browse, search, add, edit and delete recipes with full nutritional information. The app features an interactive cooking mode that utilises 7 different device hardware sensors, providing a unique and engaging user experience. Recipe images are loaded from the internet, demonstrating networking capability. The app follows WCAG 2.1 accessibility guidelines and supports dark/light themes with adjustable font sizes.

---

## Key Features

### Recipe Management
- Browse recipes with Food/Drink category filtering (All, Food, Drink)
- Real-time search by recipe name or description
- Add new recipes with cover photo capture via device camera
- Edit and delete existing recipes
- Swipe-left actions for quick edit/delete (SwipeView)
- Pull-to-refresh recipe list (RefreshView)
- Pinch-to-zoom recipe images (PinchGesture, 1x-4x scale)
- Nutritional information display per recipe (Calories, Protein, Carbs, Fat)
- Sub-category filtering (Breakfast, Lunch, Dinner, Dessert, Snack, Hot Drink, Cold Drink, Smoothie, Juice)
- Sample recipes with network-loaded images on first launch

### Hardware Features (7 total)
1. **Accelerometer** - Tilt phone to simulate pouring ingredients with real-time angle-to-progress mapping (InteractivePage)
2. **Shake Detection** - Shake to mix ingredients in cooking mode; shake on HomePage to earn discount coupons (dual-scene usage)
3. **Gyroscope** - Rotate phone to simulate stirring with rotation speed driving animation (InteractivePage)
4. **Barometer** - Reads real atmospheric pressure to recommend recipes: low pressure → hot food/drinks, high pressure → cold food/drinks (HomePage)
5. **Text-to-Speech** - Step-by-step voice reading of cooking instructions with current step highlighting and cancel support (DetailPage)
6. **Camera** - Capture recipe cover photos via native Android camera intent, or pick from device gallery (AddEditPage)
7. **Compass** - "Surprise Me" feature: compass-driven spinning wheel uses real magnetic heading to randomly select a recipe (HomePage)

### Accessibility (WCAG 2.1 Compliance)
- **1.1.1 Non-text Content** - All images have SemanticProperties.Description
- **1.3.1 Info and Relationships** - Semantic heading levels and labels used throughout
- **1.4.3 Contrast (Minimum)** - Text and background colours meet 4.5:1 contrast ratio
- **1.4.4 Resize Text** - Font size adjustable: Small (13px), Medium (16px), Large (20px), Extra Large (24px)
- **2.1.1 Keyboard Accessible** - All interactive elements support keyboard navigation
- **2.4.2 Page Titled** - Each page has a clear and descriptive title
- **3.3.1 Error Identification** - Validation errors clearly identify the problem field
- **3.3.2 Labels or Instructions** - Every input field has a visible label with clear instructions
- **4.1.2 Name, Role, Value** - SemanticProperties and AutomationProperties set on all interactive elements
- Dark/Light theme toggle with preference persistence
- Full list of WCAG principles displayed in Settings page
- Help and usage instructions available in Settings

### Interactive Cooking Mode
- 3-step guided cooking process using device sensors
- Step 1: Tilt phone to pour ingredients (Accelerometer) with progress bar
- Step 2: Rotate phone to stir (Gyroscope) with spoon animation
- Step 3: Shake phone to mix (Shake Detection) with shake counter
- Visual completion celebration when all steps are done
- Reset button to start the process again
- Adapts labels based on Food vs Drink recipe type

### Validation and Error Handling
- Recipe name: required field, maximum 100 characters
- Category: must be selected
- Ingredients: at least one required
- Steps: at least one required
- Nutrition values: numeric-only keyboard prevents invalid input
- All hardware calls wrapped in try-catch with user-friendly error messages
- Database operations wrapped in try-catch
- Image loading failure handled gracefully
- No programmer-facing error messages (e.g. no "NullReferenceException")

### Networking
- Recipe images loaded from the internet (Unsplash) on first launch
- Demonstrates network connectivity and image loading capability
- Internet permission configured in AndroidManifest.xml

---

## Pages

| Page | Description |
| --- | --- |
| **HomePage** | Recipe list with search, category filtering, barometer recommendation, shake-to-earn coupon, compass surprise wheel |
| **DetailPage** | Full recipe details with nutrition info, ingredients, steps, TTS reading, pinch-to-zoom image, edit/delete/cook buttons |
| **AddEditPage** | Form for adding/editing recipes with camera photo capture, input validation, category/sub-category selection |
| **InteractivePage** | 3-step interactive cooking using accelerometer, gyroscope and shake sensors |
| **SettingsPage** | Dark/light theme toggle, font size adjustment, WCAG accessibility info, help section, about page |

---

## Development Plan

| Phase | Description | Status |
| --- | --- | --- |
| Phase 1 | Main page structure, Shell navigation, data models, SQLite CRUD, sample data | ✅ Complete |
| Phase 2 | 7 hardware features, network images, error handling improvements | ✅ Complete |
| Phase 3 | Theme, font settings, WCAG accessibility polish | 🔄 In Progress |
| Phase 4 | Bug fixes, validation, error handling, optimisation, final README | ⏳ Planned |

---

## Tech Stack
- **.NET MAUI** (.NET 8) - Cross-platform UI framework
- **CommunityToolkit.Mvvm** (v8.4.2) - MVVM architecture with ObservableObject, RelayCommand
- **CommunityToolkit.Maui** (v9.1.0) - UI enhancements and code analysers
- **SQLite** (sqlite-net-pcl v1.9.172) - Local database storage
- **XAML** - All UI definitions using extensible markup

---

## Architecture
The app follows the **MVVM (Model-View-ViewModel)** architecture pattern:
- **Models** - Data classes (Recipe, Coupon) with SQLite attributes
- **Views** - XAML pages with code-behind for hardware sensor management
- **ViewModels** - Business logic with CommunityToolkit.Mvvm source generators
- **Services** - Database service (IDatabaseService/DatabaseService), Camera service (CameraService)
- **Converters** - 7 custom XAML value converters for data binding
- **Dependency Injection** - All services and pages registered in MauiProgram.cs

## Project Structure

```text
TasteHub/
├── Models/
│   ├── Recipe.cs                  # Recipe data model with nutrition fields
│   └── Coupon.cs                  # Coupon model for shake-to-earn feature
├── ViewModels/
│   ├── BaseViewModel.cs           # Base class with IsBusy and Title
│   ├── HomeViewModel.cs           # Recipe list, search, barometer, shake, compass
│   ├── DetailViewModel.cs         # Recipe details, TTS, pinch-to-zoom
│   ├── AddEditViewModel.cs        # Recipe form, validation, camera
│   ├── InteractiveViewModel.cs    # Accelerometer, gyroscope, shake cooking
│   └── SettingsViewModel.cs       # Theme, font size, preferences
├── Views/
│   ├── HomePage.xaml/.cs          # Main recipe list page
│   ├── DetailPage.xaml/.cs        # Recipe detail page
│   ├── AddEditPage.xaml/.cs       # Add/edit recipe page
│   ├── InteractivePage.xaml/.cs   # Interactive cooking page
│   └── SettingsPage.xaml/.cs      # Settings and accessibility page
├── Services/
│   ├── IDatabaseService.cs        # Database interface
│   ├── DatabaseService.cs         # SQLite implementation with sample data
│   └── CameraService.cs           # Camera activity result handler
├── Converters/
│   └── Converters.cs              # 7 value converters
├── Platforms/
│   └── Android/
│       ├── MainActivity.cs        # Camera intent result handling
│       └── AndroidManifest.xml    # Permissions (Internet, Camera, Storage)
├── Resources/
│   ├── Styles/Colors.xaml         # Colour definitions
│   ├── Styles/Styles.xaml         # Global styles
│   ├── Fonts/                     # OpenSans fonts
│   └── Images/                    # App images
├── App.xaml/.cs                   # Global resources, theme loading
├── AppShell.xaml/.cs              # Shell navigation with tab bar
├── MauiProgram.cs                 # Dependency injection configuration
└── README.md                      # This file
```

## Prerequisites
- Visual Studio 2022 Professional (v17.14.27)
- .NET 9 SDK (v9.0.311)
- .NET MAUI workload installed
- Android SDK Platform 34
- Android Emulator 35.5.10
- Android device or emulator (minimum API 21)
- NuGet packages:
  - CommunityToolkit.Maui 9.1.0
  - CommunityToolkit.Mvvm 8.4.2
  - sqlite-net-pcl 1.9.172
  - SQLitePCLRaw.bundle_green 2.1.11

---

## How to Run
1. Clone this repository
2. Open `TasteHub.sln` in Visual Studio 2022
3. Ensure the .NET MAUI workload is installed via Visual Studio Installer
4. Restore NuGet packages (right-click solution → Restore NuGet Packages)
5. Select an Android device or emulator as the deployment target
6. Press F5 to build and deploy
7. On first launch, sample recipes with network images will be loaded automatically

---

## Deployment
- **Android phone (HUAWEI Mate X5)** - Primary development and testing device (HarmonyOS 4.0, Android 13)
- **Android tablet (folded/unfolded Mate X5)** - Secondary deployment for responsive layout testing

---

## Version History
- **v0.1** - Phase 1: Main page structure, Shell navigation, data models, SQLite CRUD, sample data seeding
- **v0.2** - Phase 2: 7 hardware features (accelerometer, shake, gyroscope, barometer, TTS, camera, compass), network images, comprehensive error handling, .gitignore cleanup