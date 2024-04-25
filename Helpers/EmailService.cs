using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using AppBlazor.Shared.DTOs;
using AppBlazor.Shared.Entidades;

namespace AppBlazor.Server.Helpers
{
    public class EmailService : IEmailService
    {
        //ESTA ES UNA API

        //leer appsetting.json
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        //configuracion del correo
        public void SendEmail(EmailDTO request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));//el correo se envia desde aqui
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = request.Contenido
            };

            //contenido del servidor para que se pueda enviar lo anterios
            using var smtp = new SmtpClient();
            smtp.Connect(
                _config.GetSection("Email:Host").Value,
                Convert.ToInt32(_config.GetSection("Email:Port").Value),
                SecureSocketOptions.StartTls
                );

            smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:Password").Value);

            //enviar correo y enviarlo
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
