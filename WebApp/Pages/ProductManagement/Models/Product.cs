using System.ComponentModel.DataAnnotations;

namespace WebAppAuthentication.Pages.ProductManagement.Models
{
	public class Product
	{
		public int Id { get; set; }

		[Required, StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required, DataType(DataType.Currency)]
		public decimal Price { get; set; }

		[StringLength(500)]
		public string? Description { get; set; }

		// Stores the relative path of the saved image (e.g., "/uploads/product.jpg")
		public string? ImageUrl { get; set; }
	}
}
