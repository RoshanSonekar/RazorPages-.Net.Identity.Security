using Microsoft.EntityFrameworkCore;
using UserProfileService.Data.DTO;
using UserProfileService.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UserProfileService.Data
{
	public class ApplicationDbContext:DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<DtoUserProfile> DtoUserProfile { get; set; }
		//public DbSet<AddressModel> AddressModel { get; set; }
		//public DbSet<CustomFieldsModel> CustomFieldsModel { get; set; } 
	}
}
