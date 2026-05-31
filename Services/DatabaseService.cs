using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TasteHub.Models;

namespace TasteHub.Services
{
    /// <summary>
    /// SQLite database service implementing all CRUD operations
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private const string DatabaseFilename = "TasteHub.db3";

        /// <summary>
        /// Full path to the SQLite database file
        /// </summary>
        private static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        /// <summary>
        /// Initialise the database connection and create tables if they do not exist
        /// </summary>
        private async Task InitAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath);
            await _database.CreateTableAsync<Recipe>();
            await _database.CreateTableAsync<Coupon>();

            // Seed sample data if the database is empty
            int recipeCount = await _database.Table<Recipe>().CountAsync();
            if (recipeCount == 0)
            {
                await SeedSampleDataAsync();
            }
        }

        /// <summary>
        /// Insert sample recipes with network images so the app is not empty on first launch.
        /// Images are loaded from the internet to demonstrate networking capability.
        /// </summary>
        private async Task SeedSampleDataAsync()
        {
            var sampleRecipes = new List<Recipe>
            {
                new Recipe
                {
                    Name = "Classic Pancakes",
                    Category = "Food",
                    SubCategory = "Breakfast",
                    Description = "Fluffy golden pancakes perfect for a weekend breakfast.",
                    Ingredients = "[\"200g flour\",\"2 eggs\",\"300ml milk\",\"30g butter\",\"2 tbsp sugar\",\"1 tsp baking powder\"]",
                    Steps = "[\"Mix flour, sugar and baking powder in a bowl\",\"Whisk eggs and milk together, then pour into dry ingredients\",\"Melt butter and stir into the batter\",\"Heat a non-stick pan over medium heat\",\"Pour batter and cook until bubbles appear, then flip\",\"Serve with maple syrup and fresh berries\"]",
                    ImagePath = "https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=400",
                    Calories = 350, Protein = 10, Carbs = 45, Fat = 14,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Caesar Salad",
                    Category = "Food",
                    SubCategory = "Lunch",
                    Description = "A crisp and refreshing salad with creamy Caesar dressing.",
                    Ingredients = "[\"1 head romaine lettuce\",\"50g parmesan cheese\",\"100g croutons\",\"3 tbsp Caesar dressing\",\"1 grilled chicken breast\"]",
                    Steps = "[\"Wash and chop the romaine lettuce\",\"Grill and slice the chicken breast\",\"Toss lettuce with Caesar dressing\",\"Top with croutons, parmesan and chicken\"]",
                    ImagePath = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=400",
                    Calories = 420, Protein = 30, Carbs = 20, Fat = 25,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Spaghetti Bolognese",
                    Category = "Food",
                    SubCategory = "Dinner",
                    Description = "A hearty Italian classic with rich meat sauce.",
                    Ingredients = "[\"200g spaghetti\",\"250g minced beef\",\"1 onion\",\"2 garlic cloves\",\"400g tinned tomatoes\",\"2 tbsp tomato paste\",\"1 tsp oregano\"]",
                    Steps = "[\"Cook spaghetti according to packet instructions\",\"Fry onion and garlic until soft\",\"Add minced beef and brown thoroughly\",\"Stir in tinned tomatoes, tomato paste and oregano\",\"Simmer for 20 minutes\",\"Serve sauce over spaghetti with grated parmesan\"]",
                    ImagePath = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400",
                    Calories = 580, Protein = 35, Carbs = 65, Fat = 18,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Chocolate Brownie",
                    Category = "Food",
                    SubCategory = "Dessert",
                    Description = "Rich and fudgy chocolate brownies.",
                    Ingredients = "[\"200g dark chocolate\",\"150g butter\",\"200g sugar\",\"3 eggs\",\"100g flour\",\"30g cocoa powder\"]",
                    Steps = "[\"Preheat oven to 180C\",\"Melt chocolate and butter together\",\"Whisk eggs and sugar until fluffy\",\"Fold in the chocolate mixture\",\"Sift in flour and cocoa powder, fold gently\",\"Pour into a lined baking tin and bake for 25 minutes\"]",
                    ImagePath = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400",
                    Calories = 450, Protein = 6, Carbs = 52, Fat = 26,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Iced Lemon Tea",
                    Category = "Drink",
                    SubCategory = "Cold Drink",
                    Description = "A refreshing iced tea with a zesty lemon twist.",
                    Ingredients = "[\"2 black tea bags\",\"500ml boiling water\",\"1 lemon\",\"3 tbsp honey\",\"Ice cubes\"]",
                    Steps = "[\"Brew tea bags in boiling water for 5 minutes\",\"Remove tea bags and stir in honey\",\"Let the tea cool to room temperature\",\"Squeeze lemon juice into the tea\",\"Pour over ice cubes and serve with lemon slices\"]",
                    ImagePath = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400",
                    Calories = 90, Protein = 0, Carbs = 22, Fat = 0,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Hot Chocolate",
                    Category = "Drink",
                    SubCategory = "Hot Drink",
                    Description = "A warm and comforting mug of rich hot chocolate.",
                    Ingredients = "[\"250ml whole milk\",\"30g dark chocolate\",\"1 tbsp cocoa powder\",\"1 tbsp sugar\",\"Whipped cream\",\"Marshmallows\"]",
                    Steps = "[\"Heat milk in a saucepan until simmering\",\"Add chopped chocolate and cocoa powder\",\"Stir until chocolate is fully melted\",\"Add sugar to taste\",\"Pour into a mug and top with whipped cream and marshmallows\"]",
                    ImagePath = "https://images.unsplash.com/photo-1517578239113-b03992dcdd25?w=400",
                    Calories = 320, Protein = 8, Carbs = 38, Fat = 16,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Mango Smoothie",
                    Category = "Drink",
                    SubCategory = "Cold Drink",
                    Description = "A tropical and creamy mango smoothie packed with vitamins.",
                    Ingredients = "[\"1 ripe mango\",\"1 banana\",\"200ml yoghurt\",\"100ml orange juice\",\"1 tbsp honey\"]",
                    Steps = "[\"Peel and chop the mango and banana\",\"Add all ingredients to a blender\",\"Blend until smooth and creamy\",\"Pour into a glass and serve immediately\"]",
                    ImagePath = "https://images.unsplash.com/photo-1623065422902-30a2d299bbe4?w=400",
                    Calories = 250, Protein = 6, Carbs = 55, Fat = 3,
                    CreatedAt = DateTime.Now
                },
                new Recipe
                {
                    Name = "Grilled Cheese Sandwich",
                    Category = "Food",
                    SubCategory = "Lunch",
                    Description = "A golden, crispy sandwich with melted cheese inside.",
                    Ingredients = "[\"2 slices white bread\",\"40g cheddar cheese\",\"20g butter\"]",
                    Steps = "[\"Butter one side of each bread slice\",\"Place cheese between the unbuttered sides\",\"Heat a pan over medium heat\",\"Cook sandwich until golden on each side and cheese is melted\"]",
                    ImagePath = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400",
                    Calories = 380, Protein = 15, Carbs = 30, Fat = 22,
                    CreatedAt = DateTime.Now
                }
            };

            foreach (var recipe in sampleRecipes)
            {
                await _database.InsertAsync(recipe);
            }
        }

        // ==================== Recipe Operations ====================

        /// <summary>
        /// Retrieve all recipes from the database
        /// </summary>
        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            await InitAsync();
            return await _database.Table<Recipe>().ToListAsync();
        }

        /// <summary>
        /// Retrieve recipes filtered by main category (Food or Drink)
        /// </summary>
        public async Task<List<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            await InitAsync();
            return await _database.Table<Recipe>()
                .Where(r => r.Category == category)
                .ToListAsync();
        }

        /// <summary>
        /// Search recipes by name or description
        /// </summary>
        public async Task<List<Recipe>> SearchRecipesAsync(string query)
        {
            await InitAsync();
            string lowerQuery = query.ToLower();
            var allRecipes = await _database.Table<Recipe>().ToListAsync();
            return allRecipes.FindAll(r =>
                r.Name.ToLower().Contains(lowerQuery) ||
                r.Description.ToLower().Contains(lowerQuery));
        }

        /// <summary>
        /// Retrieve a single recipe by its ID
        /// </summary>
        public async Task<Recipe> GetRecipeByIdAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Recipe>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Insert a new recipe or update an existing one
        /// </summary>
        public async Task<int> SaveRecipeAsync(Recipe recipe)
        {
            await InitAsync();
            if (recipe.Id != 0)
            {
                return await _database.UpdateAsync(recipe);
            }
            else
            {
                recipe.CreatedAt = DateTime.Now;
                return await _database.InsertAsync(recipe);
            }
        }

        /// <summary>
        /// Delete a recipe from the database
        /// </summary>
        public async Task<int> DeleteRecipeAsync(Recipe recipe)
        {
            await InitAsync();
            return await _database.DeleteAsync(recipe);
        }

        // ==================== Coupon Operations ====================

        /// <summary>
        /// Retrieve all coupons from the database
        /// </summary>
        public async Task<List<Coupon>> GetAllCouponsAsync()
        {
            await InitAsync();
            return await _database.Table<Coupon>().ToListAsync();
        }

        /// <summary>
        /// Save a new coupon to the database
        /// </summary>
        public async Task<int> SaveCouponAsync(Coupon coupon)
        {
            await InitAsync();
            return await _database.InsertAsync(coupon);
        }
    }
}