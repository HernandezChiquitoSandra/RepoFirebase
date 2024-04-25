using AppBlazor.Server.Helpers;
using AppBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppBlazor.Server.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuariosController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public UsuariosController(AplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioDTO>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = context.Users.AsQueryable();//DbSet que representa la tabla de usuarios
            await HttpContext.InsertarParametrosPaginacionEnRespuesta(queryable,
                paginacion.CantidadRegistros);
            return await queryable.Paginar(paginacion).Select(x => new UsuarioDTO { Id = x.Id, Email = x.Email }).ToListAsync();
        }

        [HttpGet("roles")]
        public async Task<ActionResult<List<RolDTO>>> Get()
        {
            return await context.Roles.Select(x => new RolDTO { Nombre = x.Name! }).ToListAsync();
        }

        [HttpPost("asignarRol")]
        public async Task<ActionResult> AsignarRolUsuario(EditarRolDTO editarRolDTO)
        {
            var usuario = await userManager.FindByIdAsync(editarRolDTO.UsuarioId);

            if (usuario is null)
            {
                return BadRequest("Usuario no existe");
            }

            await userManager.AddToRoleAsync(usuario, editarRolDTO.Rol);//RoleAsync por defecto para agregar rol
            return NoContent();
        }


        [HttpPost("removerRol")]
        public async Task<ActionResult> RemoverRolUsuario(EditarRolDTO editarRolDTO)
        {
            var usuario = await userManager.FindByIdAsync(editarRolDTO.UsuarioId);

            if (usuario is null)
            {
                return BadRequest("Usuario no existe");
            }

            await userManager.RemoveFromRoleAsync(usuario, editarRolDTO.Rol);
            return NoContent();
        }

        //
    }
}
