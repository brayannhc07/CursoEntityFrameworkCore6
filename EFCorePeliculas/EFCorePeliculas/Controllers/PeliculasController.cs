using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCorePeliculas.DTOs;
using EFCorePeliculas.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Controllers {
	[ApiController]
	[Route( "api/peliculas" )]
	public class PeliculasController: ControllerBase {
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public PeliculasController( ApplicationDbContext context, IMapper mapper ) {
			this.context = context;
			this.mapper = mapper;
		}

		[HttpGet( "{id:int}" )]
		public async Task<ActionResult<PeliculaDTO>> Get( int id ) {
			var pelicula = await context.Peliculas
				.Include( pelicula => pelicula.Generos.OrderByDescending( genero => genero.Nombre ) )
				.Include( pelicula => pelicula.SalasCine )
					.ThenInclude( sala => sala.Cine )
				.Include( pelicula => pelicula.PeliculasActores
					.Where( peliculaActor => peliculaActor.Actor.FechaNacimiento.Value.Year >= 1980 ) )
					.ThenInclude( peliculaActor => peliculaActor.Actor )
				.FirstOrDefaultAsync( pelicula => pelicula.Id == id );

			if( pelicula is null ) {
				return NotFound();
			}

			var peliculaDTO = mapper.Map<PeliculaDTO>( pelicula );

			peliculaDTO.Cines = peliculaDTO.Cines.DistinctBy( x => x.Id ).ToList();

			return peliculaDTO;
		}


		[HttpGet( "conprojectto/{id:int}" )]
		public async Task<ActionResult<PeliculaDTO>> GetProjectTo( int id ) {
			var pelicula = await context.Peliculas
				.ProjectTo<PeliculaDTO>( mapper.ConfigurationProvider )
				.FirstOrDefaultAsync( pelicula => pelicula.Id == id );

			if( pelicula is null ) {
				return NotFound();
			}

			pelicula.Cines = pelicula.Cines.DistinctBy( x => x.Id ).ToList();

			return pelicula;
		}

		[HttpGet( "cargadoselectivo/{id:int}" )]
		public async Task<ActionResult> GetSelectivo( int id ) {
			var pelicula = await context.Peliculas.Select( p =>
			new {
				p.Id,
				p.Titulo,
				Generos = p.Generos.OrderByDescending( g => g.Nombre )
					.Select( g => g.Nombre ).ToList(),
				CantidadActores = p.PeliculasActores.Count(),
				CantidadCines = p.SalasCine.Select( s => s.CineId ).Distinct().Count()
			} ).FirstOrDefaultAsync( p => p.Id == id );

			if( pelicula is null ) {
				return NotFound();
			}

			return Ok( pelicula );
		}

		[HttpGet( "cargadoexplicito/{id:int}" )]
		public async Task<ActionResult<PeliculaDTO>> GetExplicito( int id ) {
			var pelicula = await context.Peliculas.AsTracking()
				.FirstOrDefaultAsync( p => p.Id == id );

			await context.Entry( pelicula ).Collection( p => p.Generos ).LoadAsync();

			var cantidadGeneros = await context.Entry( pelicula )
				.Collection( p => p.Generos ).Query().CountAsync();

			if( pelicula is null ) {
				return NotFound();
			}

			var peliculaDTO = mapper.Map<PeliculaDTO>( pelicula );

			return peliculaDTO;
		}

		[HttpGet( "lazyloading" )]
		public async Task<ActionResult<List<PeliculaDTO>>> GetLazyLoading() {
			var peliculas = await context.Peliculas.AsTracking().ToListAsync();

			foreach( var pelicula in peliculas ) {
				// Cargar los géneros de la película

				// Problema n + 1
				pelicula.Generos.ToList();
			}

			var peliculaDTOs = mapper.Map<List<PeliculaDTO>>( peliculas );

			return peliculaDTOs;
		}

		[HttpGet( "agrupadasPorEstreno" )]
		public async Task<ActionResult> GetAgrupadasPorCartelera() {
			var peliculasAgrupadas = await context.Peliculas
				.GroupBy( p => p.EnCartelera )
				.Select( g => new {
					EnCartelera = g.Key,
					Conteo = g.Count(),
					Peliculas = g.ToList()
				} ).ToListAsync();

			return Ok( peliculasAgrupadas );
		}

		[HttpGet( "agrupadasPorCantidadDeGeneros" )]
		public async Task<ActionResult> GetAgrupadasPorCantidadDeGeneros() {
			var peliculasAgrupadas = await context.Peliculas
				.GroupBy( p => p.Generos.Count() )
				.Select( g => new {
					Conteo = g.Key,
					Titulos = g.Select( x => x.Titulo ),
					Generos = g.Select( p => p.Generos )
						.SelectMany( gen => gen )
						.Select( gen => gen.Nombre )
						.Distinct()
				} ).ToListAsync();

			return Ok( peliculasAgrupadas );
		}

		[HttpGet( "filtrar" )]
		public async Task<ActionResult<List<PeliculaDTO>>> Filtrar(
			[FromQuery] PeliculasFiltroDTO peliculasFiltro ) {
			var peliculasQueryable = context.Peliculas.AsQueryable();

			if( !string.IsNullOrEmpty( peliculasFiltro.Titulo ) ) {
				peliculasQueryable = peliculasQueryable
					.Where( p => p.Titulo.Contains( peliculasFiltro.Titulo ) );
			}

			if( peliculasFiltro.EnCartelera ) {
				peliculasQueryable = peliculasQueryable
					.Where( p => p.EnCartelera );
			}

			if( peliculasFiltro.ProximosEstrenos ) {
				var today = DateTime.Today;
				peliculasQueryable = peliculasQueryable
					.Where( p => p.FechaEstreno > today );
			}

			if( peliculasFiltro.GeneroId != 0 ) {
				peliculasQueryable = peliculasQueryable
					.Where( p => p.Generos
						.Select( g => g.Identificador )
						.Contains( peliculasFiltro.GeneroId ) );
			}

			var peliculas = await peliculasQueryable
				.Include( p => p.Generos )
				.ToListAsync();

			return mapper.Map<List<PeliculaDTO>>( peliculas );
		}
	}
}
