using AppBlazor.Client.Repositorios;
using AppBlazor.Shared.DTOs;
using AppBlazor.Shared.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppBlazor.Server.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;//crear un usuario, representa un user
        private readonly SignInManager<IdentityUser> signInManager;//permite autentucar
        private readonly IConfiguration configuration;//acceder al token

        public CuentasController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }//se acaba cponstructor

        [HttpPost("crear")]
        public async Task<ActionResult<UserTokenDTO>> CreateUser([FromBody] UserInfo model)
        {
            var usuario = new IdentityUser { UserName = model.Email, Email = model.Email };
            var resultado = await userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                return await BuildToken(model);
            }
            else
            {
                return BadRequest(resultado.Errors.First());
            }
        }

        private async Task<UserTokenDTO> BuildToken(UserInfo userInfo)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userInfo.Email),

                new Claim("miValor", "Lo que yo quiera")
            };

            var usuario = await userManager.FindByEmailAsync(userInfo.Email);
            var roles = await userManager.GetRolesAsync(usuario!);

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var token = new JwtSecurityToken(//construccion del token
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
                );

            return new UserTokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }


        //metodo de login
        [HttpPost("Login")]
        public async Task<ActionResult<UserTokenDTO>> Login([FromBody] UserInfo model)
        {
            var resultado = await signInManager.PasswordSignInAsync(model.Email,
                model.Password, isPersistent: false, lockoutOnFailure: false);//si el usuario se equivoca mucho la cuenta se cierra = false

            if (resultado.Succeeded)
            {
                return await BuildToken(model);
            }
            else
            {
                return BadRequest("Intento de login fallido");
            }
        }


        [HttpGet("verificarcorreo")]
        public async Task<ActionResult<bool>> VerificarCorreoExistente(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpPost("RecuperarContrasena")]
        public async Task<ActionResult<string>> RecuperarContrasena([FromBody] string email)
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                return NotFound();
            }

            // Generar un token único para restablecer la contraseña
            var token = await userManager.GeneratePasswordResetTokenAsync(usuario);

            // Construir el enlace para la página de restablecimiento de contraseña
            var callbackUrl = $"{Request.Scheme}://{Request.Host}/RecuperarContrasenia?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            return Ok(callbackUrl); // Devolver el callbackUrl como parte de la respuesta
        }


        [HttpPost("RestablecerContrasena")]
        public async Task<ActionResult> RestablecerContrasena([FromBody] CambioContraseniaInfo cambioContraseniaInfo)
        {
            var usuario = await userManager.FindByEmailAsync(cambioContraseniaInfo.Email);
            if (usuario == null)
            {
                // El usuario no existe
                return NotFound();
            }

            // Verificar que el token de restablecimiento de contraseña sea válido
            var result = await userManager.ResetPasswordAsync(usuario, cambioContraseniaInfo.Token, cambioContraseniaInfo.NuevaContrasenia);
            if (result.Succeeded)
            {
                // La contraseña se actualizó correctamente
                return Ok();
            }
            else
            {
                // Hubo un error al actualizar la contraseña
                return BadRequest("Error al restablecer la contraseña.");
            }
        }





    }
}