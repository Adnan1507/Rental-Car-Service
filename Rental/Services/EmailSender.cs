using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Rental.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Email:Address"],
                    _config["Email:Password"])
            };

            var mailMessage = new MailMessage()
            {
                From = new MailAddress(_config["Email:Address"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtp.SendMailAsync(mailMessage);
        }
    }
}
