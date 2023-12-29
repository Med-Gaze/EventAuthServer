using MailKit.Net.Smtp;
using MailKit.Security;
using med.common.library.Enum;
using med.common.library.model;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace med.common.library.configuration.service
{
    public class EmailService : IEmailService
    {

        public async Task<string> SendMail(EmailViewModel model, EmailConfiguration emailConfiguration)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailConfiguration.EmailDisplayName, emailConfiguration.EmailFrom));
                message.To.Add(new MailboxAddress(model.EmailTo, model.EmailTo));
                if (model.EmailCC.Count > 0)
                {
                    foreach (var cc in model.EmailCC)
                    {
                        message.Cc.Add(new MailboxAddress(cc, cc));
                    }
                }

                message.Subject = model.Subject;
                var textFormat = model.ContentType switch
                {
                    (int)EmailContentTypeEnum.Html => TextFormat.Html,
                    (int)EmailContentTypeEnum.Text => TextFormat.Text,
                    _ => TextFormat.Plain,
                };
                message.Body = new TextPart(textFormat)
                {
                    Text = model.Content
                };

                using var client = new SmtpClient();
                int port = Convert.ToInt32(emailConfiguration.Port);
                client.ServerCertificateValidationCallback = (sender, certificate, certChainType, errors) => true;
                await client.ConnectAsync(emailConfiguration.Server, port, SecureSocketOptions.Auto).ConfigureAwait(false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(emailConfiguration.Username, emailConfiguration.Password).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                client.Dispose();
                return $"Email successfully send to {model.EmailTo}";
            }
            catch
            {
                throw;
            }
        }
    }
}
