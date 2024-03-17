using System.Net.Http.Headers;

namespace Listem.Mobile.Utilities;

public static class HttpClientExtensions
{
    public static void SetDefaultHeaders(this HttpClient httpClient, string? accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }
}
