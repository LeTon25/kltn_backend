using KLTN.Domain.Services;
using KLTN.Domain.Settings;
using MailKit.Net.Smtp;
using MimeKit;


namespace KLTN.Infrastructure.Mailing
{
    public class SmtpEmailService : ISMTPEmailService
    {
        private readonly ISMTPEmailSettings _settings;
        private readonly SmtpClient _smtpClient;
        public SmtpEmailService(ISMTPEmailSettings settings,SmtpClient smtpClient)
        {
            this._settings = settings;
            this._smtpClient = smtpClient;
        }
        public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = default)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(_settings.DisplayName,request.From ?? _settings.From),
                Subject  = request.Subject,
                Body = new BodyBuilder()
                {
                    HtmlBody = request.Body
                }.ToMessageBody(),
            };

            if (request.ToAddresses.Any())
            {
                foreach (var address in request.ToAddresses)
                {
                    emailMessage.To.Add(MailboxAddress.Parse(address));
                }
            }
            else
            {
                var toAddress = request.ToAddress;
                emailMessage.To.Add(MailboxAddress.Parse(toAddress));
            }
            try
            {
                await _smtpClient.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);
                await _smtpClient.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
                await _smtpClient.SendAsync(emailMessage,cancellationToken);
                await _smtpClient.DisconnectAsync(true,cancellationToken);
            }
            catch (Exception ex) 
            { 
                
            }
            finally
            {

            }
        }
    }
}
