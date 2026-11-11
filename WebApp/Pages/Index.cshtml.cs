using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
	[Authorize]
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnGet()
		{
		}

		// Handles the top search box interaction
		public void OnGetSearch(string searchQuery)
		{
			_logger.LogInformation($"User searching for: {searchQuery}");

			// TODO: Pass 'searchQuery' to your repository/database layer 
			// to filter the product list displayed in the grid below.
		}

		public IActionResult OnPostAddToCart(int productId)
		{
			return RedirectToPage();
		}

		public IActionResult OnPostAddToWishlist(int productId)
		{
			return RedirectToPage();
		}
	}
}