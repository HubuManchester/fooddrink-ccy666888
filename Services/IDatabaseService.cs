using System.Collections.Generic;
using System.Threading.Tasks;
using TasteHub.Models;

namespace TasteHub.Services
{
    /// <summary>
    /// Interface for database operations on recipes and coupons
    /// </summary>
    public interface IDatabaseService
    {
        // Recipe CRUD operations
        Task<List<Recipe>> GetAllRecipesAsync();
        Task<List<Recipe>> GetRecipesByCategoryAsync(string category);
        Task<List<Recipe>> SearchRecipesAsync(string query);
        Task<Recipe> GetRecipeByIdAsync(int id);
        Task<int> SaveRecipeAsync(Recipe recipe);
        Task<int> DeleteRecipeAsync(Recipe recipe);

        // Coupon operations
        Task<List<Coupon>> GetAllCouponsAsync();
        Task<int> SaveCouponAsync(Coupon coupon);
    }
}