using Microsoft.Extensions.Configuration;

namespace KLTN.Application.Services.HttpServices
{
    public class BackgroundJobHttpService
    {
        public HttpClient Client { get; }
        public BackgroundJobHttpService(HttpClient client,IConfiguration configuration)
        {
            var backgroundJobUrl = configuration.GetSection("BackgroundJobUrl").Value;
            if (backgroundJobUrl == null) throw new ArgumentNullException("Missing Configuarion for background job url");
            
            client.BaseAddress = new Uri(backgroundJobUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;
        }
    }
}
