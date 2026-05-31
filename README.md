# TasteHub - Food and Drink Recipe App

## Author
**[Your Name]** - Manchester Metropolitan University

## Module
6G6Z0014 - Mobile Computing

---

## Overview
TasteHub is a cross-platform food and drink recipe management application built with .NET MAUI.
Users can browse, search, add, edit and delete recipes with full nutritional information.
The app features an interactive cooking mode that utilises multiple device hardware sensors,
providing a unique and engaging user experience.

## Key Features

### Recipe Management
- Browse recipes with Food/Drink category filtering
- Real-time search by recipe name or description
- Add new recipes with cover photo capture
- Edit and delete existing recipes
- Swipe-left actions for quick edit/delete
- Pull-to-refresh recipe list
- Nutritional information display per recipe

### Hardware Features (7 total)
1. **Accelerometer** - Tilt phone to simulate pouring ingredients
2. **Shake Detection** - Shake to mix ingredients and earn discount coupons
3. **Gyroscope** - Rotate phone to simulate stirring
4. **Barometer** - Weather-based recipe recommendations based on air pressure
5. **Text-to-Speech** - Step-by-step voice reading of cooking instructions
6. **Camera** - Capture recipe cover photos or pick from gallery
7. **Microphone** - Voice search for recipes

### Accessibility (WCAG 2.1 Compliance)
- Adjustable font sizes: Small, Medium, Large, Extra Large
- Dark/Light theme toggle
- Screen reader support via SemanticProperties
- Colour contrast ratio meeting WCAG AA standard (4.5:1)
- Clear labels and instructions on all input fields
- Full list of WCAG principles referenced in Settings page

### Interactive Cooking Mode
- Step-by-step guided cooking using device sensors
- Pour ingredients by tilting the device
- Stir by rotating the device
- Mix by shaking the device
- Visual progress indicators for each step

## Development Plan

| Phase | Description | Status |
| --- | --- | --- |
| Phase 1 | Main page structure, navigation, data model, SQLite CRUD | Complete |
| Phase 2 | Hardware features implementation | Planned |
| Phase 3 | Theme, font settings, WCAG accessibility | Planned |
| Phase 4 | Bug fixes, validation, error handling, optimisation | Planned |

## Tech Stack
- **.NET MAUI** (.NET 8) - Cross-platform UI framework
- **CommunityToolkit.Mvvm** - MVVM architecture with source generators
- **CommunityToolkit.Maui** - UI enhancements and analysers
- **SQLite** (sqlite-net-pcl) - Local database storage
- **XAML** - All UI definitions

## Project Structure
```
TasteHub/
©À©¤©¤ Models/          # Data models (Recipe, Coupon)
©À©¤©¤ ViewModels/      # MVVM view models
©À©¤©¤ Views/           # XAML pages
©À©¤©¤ Services/        # Database and hardware services
©À©¤©¤ Converters/      # XAML value converters
©À©¤©¤ Resources/       # Images, fonts, styles
©À©¤©¤ App.xaml         # Global resources and themes
©À©¤©¤ AppShell.xaml    # Navigation structure
©¸©¤©¤ MauiProgram.cs   # Dependency injection setup
```

## Prerequisites
- Visual Studio 2022 (v17.8+)
- .NET 8 SDK
- .NET MAUI workload installed
- Android SDK (API 21+)

## How to Run
1. Clone this repository
2. Open `TasteHub.sln` in Visual Studio 2022
3. Ensure .NET MAUI workload is installed
4. Select an Android device/emulator or Windows Machine as target
5. Press F5 to build and deploy

## Deployment
- **Android phone** - Primary development and testing device
- **Android tablet** - Secondary deployment for responsive layout testing

## Version History
- v1.0 - Initial release with full recipe management and 7 hardware features
