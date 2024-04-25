using AppBlazor.Server.Helpers;
using AppBlazor.Shared.DTOs;
using AppBlazor.Shared.Entidades;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AppBlazor.Shared.DTOs.HomePageDTO;



namespace AppBlazor.Server.Controllers
{
    [Route("api/peliculas")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PeliculasController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;
        private readonly string contenedor = "posters";
        public PeliculasController(AplicationDbContext context, IAlmacenadorArchivos almacenadorArchivos, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.almacenadorArchivos = almacenadorArchivos;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Pelicula>>> Get()
        //{
        //    return await context.Peliculas.ToListAsync();//toList para todos los generos
        //}



        [HttpGet]
        [AllowAnonymous]//permitir a anomonimos
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]//ahora solo una persona con token puede acceder al endpoint (metodo get)
        public async Task<ActionResult<Shared.DTOs.HomePageDTO>> Get()
        {
            var limite = 6;

            var peliculasEnCartelera = await context.Peliculas
                .Where(pelicula => pelicula.EnCartelera).Take(limite)
                .OrderByDescending(pelicula => pelicula.FechaLanzamiento).ToListAsync();

            var fechaActual = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .Where(pelicula => pelicula.FechaLanzamiento > fechaActual)
                .OrderBy(pelicula => pelicula.FechaLanzamiento).Take(limite)
                .ToListAsync();

            var resultado = new Shared.DTOs.HomePageDTO
            {
                PeliculasEnCartelera = peliculasEnCartelera,
                ProximosEstrenos = proximosEstrenos
            };

            return resultado;
        }


        //para ver pelicula
        [HttpGet("{id:int}")]
        [AllowAnonymous] //cambiar despues
        public async Task<ActionResult<PeliculaVisualizarDTO>> Get(int id)
        {
            var pelicula = await context.Peliculas.Where(pelicula => pelicula.Id == id)
                .Include(pelicula => pelicula.GenerosPelicula)
                    .ThenInclude(gp => gp.Genero)
                .FirstOrDefaultAsync();//traer uno solo

            if (pelicula is null)
            {
                return NotFound();
            }

            // TODO: Sistema de votación
            var promedioVoto =0.0;
            var votoUsuario = 0;

            if (await context.VotosPeliculas.AnyAsync(x => x.PeliculaId == id))
            {
                promedioVoto = await context.VotosPeliculas.Where(x => x.PeliculaId == id)
                    .AverageAsync(x => x.Voto);//sacar el promedio

                //HttpContext.User.Identity.IsAuthenticated
                if (HttpContext.User.Identity!.IsAuthenticated)
                {
                    var usuario = await userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

                    if (usuario is null)
                    {
                        return BadRequest("user no encontrado");
                    }
                    var usuarioId = usuario.Id;

                    var votoUsuarioDB = await context.VotosPeliculas
                        .FirstOrDefaultAsync(x => x.PeliculaId == id && x.usuarioId == usuarioId);

                    if (votoUsuarioDB is not null) {
                        votoUsuario = votoUsuarioDB.Voto;
                    }
                }
            }

            var modelo = new PeliculaVisualizarDTO();
            modelo.Pelicula = pelicula;
            modelo.Generos = pelicula.GenerosPelicula.Select(gp => gp.Genero!).ToList();//tomar los generos de genero
            //modelo.Actores = pelicula.PeliculasActor.Select(pa => new Actor
            //{
            //    Nombre = pa.Actor!.Nombre,
            //    Foto = pa.Actor.Foto,
            //    Personaje = pa.Personaje,
            //    Id = pa.ActorId
            //}).ToList();

            modelo.PromedioVotos = promedioVoto;
            modelo.VotoUsuario = votoUsuario;
            return modelo;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(Pelicula pelicula)
        {
            if (!string.IsNullOrWhiteSpace(pelicula.Poster))
            {
                var poster = Convert.FromBase64String(pelicula.Poster);
                pelicula.Poster = await almacenadorArchivos.GuardarArchivo(poster, ".jpg", contenedor);
            }

            context.Add(pelicula);
            await context.SaveChangesAsync();
            return pelicula.Id;
        }

        [HttpGet("actualizar/{id}")]
        public async Task<ActionResult<PeliculaActualizacionDTO>> PutGet(int id)
        {
            var peliculaActionResult = await Get(id);
            if (peliculaActionResult.Result is NotFoundResult) { return NotFound(); }

            var peliculaVisualizarDTO = peliculaActionResult.Value;
            var generosSeleccionadosIds = peliculaVisualizarDTO!.Generos.Select(x => x.Id).ToList();
            var generosNoSeleccionados = await context.Generos
                .Where(x => !generosSeleccionadosIds.Contains(x.Id))
                .ToListAsync();

            var modelo = new PeliculaActualizacionDTO();
            modelo.Pelicula = peliculaVisualizarDTO.Pelicula;
            modelo.GenerosNoSeleccionados = generosNoSeleccionados;
            modelo.GenerosSeleccionados = peliculaVisualizarDTO.Generos;
           // modelo.Actores = peliculaVisualizarDTO.Actores;
            return modelo;
        }

        [HttpPut]
        public async Task<ActionResult> Put(Pelicula pelicula)
        {
            var peliculaDB = await context.Peliculas
                .Include(x => x.GenerosPelicula)
                //.Include(x => x.PeliculasActor)
                .FirstOrDefaultAsync(x => x.Id == pelicula.Id);

            if (peliculaDB is null)
            {
                return NotFound();
            }
            peliculaDB = mapper.Map(pelicula, peliculaDB);

            if (!string.IsNullOrWhiteSpace(pelicula.Poster))
            {
                var posterImagen = Convert.FromBase64String(pelicula.Poster);
                peliculaDB.Poster = await almacenadorArchivos.EditarArchivo(posterImagen,
                    ".jpg", contenedor, peliculaDB.Poster!);
            }

            //EscribirOrdenActores(peliculaDB);

            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pelicula = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula is null)
            {
                return NotFound();
            }

            context.Remove(pelicula);
            await context.SaveChangesAsync();
            await almacenadorArchivos.EliminarArchivo(pelicula.Poster!, contenedor);

            return NoContent();
        }

        //
        [AllowAnonymous]
        [HttpGet("filtrar")]
        public async Task<ActionResult<List<Pelicula>>> Get(
            [FromQuery] ParametrosBusquedaPeliculasDTO modelo)
        {
            var peliculasQueryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(modelo.Titulo))
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.Titulo.Contains(modelo.Titulo));
            }

            if (modelo.EnCartelera)
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.EnCartelera);
            }

            if (modelo.Estrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable.Where(x => x.FechaLanzamiento >= hoy);
            }

            if (modelo.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                                        .Where(x => x.GenerosPelicula
                                            .Select(y => y.GeneroId)
                                            .Contains(modelo.GeneroId));
            }

            if (modelo.MasVotadas)
            {
                peliculasQueryable = peliculasQueryable
                                        .Where(x => context.VotosPeliculas
                                            .Where(vp => vp.PeliculaId == x.Id)
                                            .Average(vp => vp.Voto) > 0); // Filtrar por películas con votos
                peliculasQueryable = peliculasQueryable
                                        .OrderByDescending(x => context.VotosPeliculas
                                            .Where(vp => vp.PeliculaId == x.Id)
                                            .Average(vp => vp.Voto)); // Ordenar por promedio de votos descendente
            }

            await HttpContext.InsertarParametrosPaginacionEnRespuesta(peliculasQueryable,
                    modelo.CantidadRegistros);

            var peliculas = await peliculasQueryable.Paginar(modelo.PaginacionDTO).ToListAsync();
            return peliculas;
        }

    }
}
