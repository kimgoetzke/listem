using System.Net;
using System.Net.Http.Json;
using Listem.Contracts;

namespace Listem.Utilities;

public static class HttpUtilities
{
    public static async Task<string> ParseErrorResponse(HttpResponseMessage response, string url)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "Not authorised - you need to sign in";
        }
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Logger.Log($"Error response from {url}: {errorResponse}");
        return errorResponse!.Errors?.Values.First().First()
            ?? "Sorry, something went wrong - please try again";
    }
}
