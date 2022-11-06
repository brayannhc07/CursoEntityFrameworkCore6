using Microsoft.EntityFrameworkCore;

namespace DemoEFCore6 {
	internal class ApplicationDbContext: DbContext {
		protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder ) {
			optionsBuilder.UseSqlServer( "Server=127.0.0.1;Database=DemoEFCore6;User id=SA;Password=yourStrong(!)Password" );
			base.OnConfiguring( optionsBuilder );
		}

		public DbSet<Persona> Personas { get; set; }

	}
}
