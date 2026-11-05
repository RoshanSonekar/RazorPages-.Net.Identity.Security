using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebAppAuthentication.Pages.ProductManagement.Models;

namespace WebAppAuthentication.Pages.ProductManagement.AddProduct
{
    public class AddProductModel : PageModel
    {
		private readonly IWebHostEnvironment _environment;

		public AddProductModel(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		[BindProperty]
		public Product product { get; set; }

		[BindProperty]
		[Display(Name = "Product Image")]
		public IFormFile? ImageFile { get; set; }

		public IActionResult OnGet()
		{
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			if (ImageFile != null && ImageFile.Length > 0)
			{
				// Ensure uploads directory exists
				string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
				Directory.CreateDirectory(uploadsFolder);

				// Create a unique file name to avoid collisions
				string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await ImageFile.CopyToAsync(fileStream);
				}

				product.ImageUrl = "/uploads/" + uniqueFileName;
			}

			// TODO: Save 'Product' to your database via Entity Framework Core context
			// _context.Products.Add(product);
			// await _context.SaveChangesAsync();

			return RedirectToPage("./Index");
		}
	}
}
