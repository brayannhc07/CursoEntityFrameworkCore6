//using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Entidades {
	public class Pelicula {
		public int Id { get; set; }
		public string Titulo { get; set; }
		public bool EnCartelera { get; set; }
		public DateTime FechaEstreno { get; set; }
		//[Unicode(false)]
		public string PosterUrl { get; set; }
		public List<Genero> Generos { get; set; }
		public HashSet<SalaCine> SalasCine { get; set; }
		public HashSet<PeliculaActor> PeliculasActores { get; set; }
	}
}
