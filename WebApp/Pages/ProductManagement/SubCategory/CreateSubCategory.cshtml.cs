using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppAuthentication.Pages.ProductManagement.Category;

namespace WebAppAuthentication.Pages.ProductManagement.SubCategory
{
  public class CreateSubCategoryModel : PageModel
	{
		[BindProperty]
		public ProductSubCategory Input { get; set; } = new();

		public void OnGet()
    {
    }
  }

  public class ProductSubCategory
  {
    public int Id { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string SubCategoryDescription { get; set; } = string.Empty;
		public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
		public bool IsActive { get; set; } = false;
  }
}
