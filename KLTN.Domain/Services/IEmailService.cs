namespace KLTN.Domain.Services
{
    public interface IEmailService<in T> where T : class
    {
        Task SendEmailAsync(T request, string? templateName = null,Dictionary<string,string>?  placeHolders = null,CancellationToken cancellationToken = new CancellationToken());
        void SendEmail(T request, string? templateName = null, Dictionary<string, string>? placeHolders = null, CancellationToken cancellationToken = new CancellationToken());
    }
  
}
