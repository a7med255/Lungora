using Lungora.Bl.Interfaces;
using Lungora.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;


namespace Lungora.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(EmailMetadata emailMetadata)
        {
            try
            {
                //var fromMail = _configuration["EmailSettings:EmailFrom"];
                //var password = _configuration["EmailSettings:EmailPassword"];
                //var host = _configuration["EmailSettings:EmailHost"];

                var toMail = emailMetadata.ToAddress;
                var subject = emailMetadata.Subject;
                var body = emailMetadata.Body;

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("ah8455545@gmail.com"));
                email.To.Add(MailboxAddress.Parse(toMail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
               await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
               await smtp.AuthenticateAsync("ah8455545@gmail.com", "byek lqlv pnvm ydwi");

               await smtp.SendAsync(email);

               await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

    }
}
