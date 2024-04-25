using AppBlazor.Shared.DTOs;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppBlazor.Server.Helpers;
using AppBlazor.Shared.Entidades;

namespace AppBlazor.Server.Controllers
{
    [ApiController]
    [Route("api/Email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        //metodo que a travez de la api se envia el cerpo del correo
        [HttpPost]
        public IActionResult SendEmail(EmailDTO request) 
        { 
            _emailService.SendEmail(request);
            return Ok();
        }

    }
}
