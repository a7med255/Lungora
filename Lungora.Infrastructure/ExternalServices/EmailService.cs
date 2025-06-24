using Lungora.Bl.Interfaces;
using Lungora.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;


namespace Lungora.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task SendEmailAsync(EmailMetadata emailMetadata)
        {
            try
            {
                var fromMail = configuration["EmailSettings:EmailFrom"];
                var password = configuration["EmailSettings:EmailPassword"];
                var host = configuration["EmailSettings:EmailHost"];

                var toMail = emailMetadata.ToAddress;
                var subject = emailMetadata.Subject;
                var body = emailMetadata.Body;

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(fromMail));
                email.To.Add(MailboxAddress.Parse(toMail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(fromMail, password);

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
