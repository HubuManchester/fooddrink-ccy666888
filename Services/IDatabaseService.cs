using System.Collections.Generic;
using System.Threading.Tasks;
using TasteHub.Models;

namespace TasteHub.Services
{
    /// <summary>
    /// Interface defining all database operations for recipes and coupons.
    /// Implemented by DatabaseService using SQLite for local storage.
    /// Used with dependency injection for testability and loose coupling.
    /// </summary>
    public interface IDatabaseService
    {
        // ==================== Recipe CRUD Operations ====================

        /// <summary>Retrieve all recipes from the database</summary>
        /// <returns>List of all Recipe objects</returns>
        Task<List<Recipe>> GetAllRecipesAsync();

        /// <summary>Retrieve recipes filtered by main category (Food or Drink)</summary>
        /// <param name="category">Category filter: "Food" or "Drink"</param>
        /// <returns>Filtered list of Recipe objects</returns>
        Task<List<Recipe>> GetRecipesByCategoryAsync(string category);

        /// <summary>Search recipes by matching name or description</summary>
        /// <param name="query">Search query string</param>
        /// <returns>List of matching Recipe objects</returns>
        Task<List<Recipe>> SearchRecipesAsync(string query);

        /// <summary>Retrieve a single recipe by its unique ID</summary>
        /// <param name="id">Recipe primary key ID</param>
        /// <returns>Recipe object or null if not found</returns>
        Task<Recipe> GetRecipeByIdAsync(int id);

        /// <summary>Insert a new recipe or update an existing one</summary>
        /// <param name="recipe">Recipe object to save</param>
        /// <returns>Number of rows affected</returns>
        Task<int> SaveRecipeAsync(Recipe recipe);

        /// <summary>Delete a recipe from the database</summary>
        /// <param name="recipe">Recipe object to delete</param>
        /// <returns>Number of rows affected</returns>
        Task<int> DeleteRecipeAsync(Recipe recipe);

        // ==================== Coupon Operations ====================

        /// <summary>Retrieve all earned coupons from the database</summary>
        /// <returns>List of all Coupon objects</returns>
        Task<List<Coupon>> GetAllCouponsAsync();

        /// <summary>Save a newly earned coupon to the database</summary>
        /// <param name="coupon">Coupon object to save</param>
        /// <returns>Number of rows affected</returns>
        Task<int> SaveCouponAsync(Coupon coupon);
    }
}