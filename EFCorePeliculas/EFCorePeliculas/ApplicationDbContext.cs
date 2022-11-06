using EFCorePeliculas.Entidades;
using EFCorePeliculas.Entidades.Configuraciones;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EFCorePeliculas {
	public class ApplicationDbContext: DbContext {
		public ApplicationDbContext( DbContextOptions options ) : base( options ) {

		}

		protected override void ConfigureConventions( ModelConfigurationBuilder configurationBuilder ) {
			configurationBuilder.Properties<DateTime>().HaveColumnType( "date" );
		}

		protected override void OnModelCreating( ModelBuilder modelBuilder ) {
			base.OnModelCreating( modelBuilder );

			//modelBuilder.ApplyConfiguration( new GeneroConfig() );
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		}

		public DbSet<Genero> Generos { get; set; }
		public DbSet<Actor> Actores { get; set; }
		public DbSet<Cine> Cines { get; set; }
		public DbSet<CineOferta> CineOfertas { get; set; }
		public DbSet<Pelicula> Peliculas { get; set; }
		public DbSet<SalaCine> SalasCines { get; set; }
		public DbSet<PeliculaActor> PeliculasActores { get; set; }
	}
}
