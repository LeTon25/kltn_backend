using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KLTN.Domain.Extensions
{
    public static class HttpClientExtension
    {
        public static async Task<T?> ReadContentAs<T>(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) 
            {
                throw new Exception($"Something went wrong calling the API : {response.ReasonPhrase}");
            }
            var dataAsString = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(dataAsString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive =true,
                ReferenceHandler = ReferenceHandler.Preserve
            });
        }    
        public static Task<HttpResponseMessage> PostAsJson<T>(this HttpClient client, string url, T data)
        { 
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return client.PostAsync(url, content);
        }
    }

    /*
        var response = await _bj.Client.PostAsJson(uri,model);
        if(response.EnsureSuccessStatusCode().IsSuccessStatusCode)
        {
            var jobId = await response.Content
        }
     */
}
