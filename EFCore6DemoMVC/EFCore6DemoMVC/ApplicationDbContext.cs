using EFCore6DemoMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore6DemoMVC {
	public class ApplicationDbContext: DbContext {
		public ApplicationDbContext( DbContextOptions options ) : base( options ) {

		}
		public DbSet<Persona> Personas { get; set; }
	}
}
