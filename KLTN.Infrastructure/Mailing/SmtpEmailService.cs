﻿using KLTN.Domain.Services;
using KLTN.Domain.Settings;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.WebSockets;


namespace KLTN.Infrastructure.Mailing
{
    public class SmtpEmailService : ISMTPEmailService
    {
        private readonly ISMTPEmailSettings _settings;
        private readonly SmtpClient _smtpClient;
        public SmtpEmailService(ISMTPEmailSettings settings)
        {
            this._settings = settings;
            this._smtpClient = new SmtpClient();
        }
        public async Task SendEmailAsync(MailRequest request,string? templateName = null, Dictionary<string, string>? placeHolders = null, CancellationToken cancellationToken = default)
        {
 
            var emailBody = request.Body;
            if(templateName != null)
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var templatePath = Path.Combine(_settings.TemplateFolderPath, $"{templateName}.html");
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template {templateName} not found.");
                }
                emailBody = await File.ReadAllTextAsync(templatePath);
                foreach (var placeholder in placeHolders)
                {
                    emailBody = emailBody.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }
            }
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From),
                Subject = request.Subject,
                Body = new BodyBuilder()
                {
                    HtmlBody = emailBody,
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