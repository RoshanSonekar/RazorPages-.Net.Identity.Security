using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebAppAuthentication.Pages.Transactions;

namespace WebAppAuthentication.Pages.ProductManagement.Category
{
    public class CreateCategoryModel : PageModel
    {
		[BindProperty]
		public ProductCategory Input { get; set; } = new();

		public CreateCategoryModel()
		{

		}
		public void OnGet()
        {
        }
    }

  public class ProductCategory
  {
    public int Id { get; set; }

		[Required(ErrorMessage = "Category name is required.")]
		[StringLength(256, ErrorMessage = "Category name cannot exceed 256 characters.")]
		public string CategoryName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Display order is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "Display order must be a positive integer.")]
		public int DisplayOrder { get; set; }

		public string CategoryDescription { get; set; } = string.Empty;
		public bool IsActive { get; set; }= false;
	}
}

 
