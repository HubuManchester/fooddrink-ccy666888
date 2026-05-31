using SQLite;
using System;

namespace TasteHub.Models
{
    /// <summary>
    /// Coupon model for the shake-to-earn coupon feature
    /// </summary>
    public class Coupon
    {
        /// <summary>Primary key, auto-increment ID</summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>Coupon code</summary>
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Coupon description</summary>
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Discount percentage (e.g. 10 means 10% off)</summary>
        public double Discount { get; set; }

        /// <summary>Whether the coupon has been used</summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>Timestamp when the coupon was obtained</summary>
        public DateTime ObtainedAt { get; set; } = DateTime.Now;
    }
}