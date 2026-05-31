using SQLite;
using System;

namespace TasteHub.Models
{
    /// <summary>
    /// Recipe data model for storing food and drink recipe information
    /// </summary>
    public class Recipe
    {
        /// <summary>Primary key, auto-increment ID</summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>Recipe name</summary>
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Main category: Food or Drink</summary>
        [MaxLength(20)]
        public string Category { get; set; } = string.Empty;

        /// <summary>Sub-category: Breakfast, Lunch, Dinner, Dessert, Cold Drink, Hot Drink, etc.</summary>
        [MaxLength(50)]
        public string SubCategory { get; set; } = string.Empty;

        /// <summary>Recipe description</summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Ingredients list stored as JSON string</summary>
        public string Ingredients { get; set; } = "[]";

        /// <summary>Cooking steps stored as JSON string</summary>
        public string Steps { get; set; } = "[]";

        /// <summary>Cover image file path</summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>Calories (kcal)</summary>
        public double Calories { get; set; }

        /// <summary>Protein in grams</summary>
        public double Protein { get; set; }

        /// <summary>Carbohydrates in grams</summary>
        public double Carbs { get; set; }

        /// <summary>Fat in grams</summary>
        public double Fat { get; set; }

        /// <summary>Record creation timestamp</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}