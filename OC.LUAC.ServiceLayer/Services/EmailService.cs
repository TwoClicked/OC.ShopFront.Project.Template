using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ServiceLayer.Services
{
    public class EmailService : IEmailService
    {

        private readonly IConfiguration _config;


        public EmailService(IConfiguration config)
        {
            _config = config;
            
        }

        public async Task SendEmailAsync(string to, string subject, string body, byte[]? pdfAttachment = null, string? attachmentName = null)
        {

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("LUAC", _config["Email:SmtpUser"]));
            email.To.Add(new MailboxAddress(to, to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body};

            if (pdfAttachment != null && attachmentName != null)
            {
                builder.Attachments.Add(attachmentName, pdfAttachment, new ContentType("application", "pdf"));
            }

            email.Body = builder.ToMessageBody();


            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:SmtpServer"], int.Parse(_config["Email:SmtpPort"]), false);
            await smtp.AuthenticateAsync(_config["Email:SmtpUser"], _config["Email:SmtpPass"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

        }
    }
}
