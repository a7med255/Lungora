using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IEmailService
    {
         Task SendEmailAsync(EmailMetadata emailMetadata);
    }
}
