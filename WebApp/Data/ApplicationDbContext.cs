using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Data.Account;

namespace WebApp.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUserExtension>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
		
	}
}
