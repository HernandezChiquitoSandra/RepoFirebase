using AppBlazor.Shared.DTOs;
using AppBlazor.Shared.Entidades;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppBlazor.Server.Controllers
{
    [ApiController]
    [Route("api/votos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VotosController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;

        public VotosController(AplicationDbContext context, 
            UserManager<IdentityUser> userManager, IMapper mapper) 
        {
            this.context = context;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        //--------------------------------------------

        [HttpPost]
        public async Task<ActionResult> Votar(VotoPeliculaDTO votoPeliculaDTO)
        {
            var usuario = await userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

            if (usuario is null)
            {
                return BadRequest("user no encontrado");
            }
            var usuarioId = usuario.Id;

            var votoActual = await context.VotosPeliculas.FirstOrDefaultAsync(x => x.PeliculaId==votoPeliculaDTO.PeliculaId
            && x.usuarioId == usuarioId);

            if (votoActual == null)
            {
                var votoPelicula = mapper.Map<VotoPelicula>(votoPeliculaDTO);

                votoPelicula.usuarioId = usuarioId;
                votoPelicula.FechaVoto = DateTime.Now;
                context.Add(votoPelicula);
                //await context.SaveChangesAsync();
            }
            else
            {//actualizar registro actual
                votoActual.FechaVoto = DateTime.Now;
                votoActual.Voto = votoPeliculaDTO.voto;
                //await context.SaveChangesAsync();
            }
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
