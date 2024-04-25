using AppBlazor.Shared.DTOs;

namespace AppBlazor.Server.Helpers
{
    public interface IEmailService
    {
        void SendEmail(EmailDTO request);// metodo
    }
}
