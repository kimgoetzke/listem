using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Listem.Shared.Contracts;

namespace Listem.Mobile.Utilities;

public static class HttpUtilities
{
    private static JsonSerializerOptions JsonOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

    public static StringContent Content<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<string> ParseErrorResponse(HttpResponseMessage response)
    {
        ErrorResponse? errorResponse;
        try
        {
            errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        }
        catch (JsonException)
        {
            Logger.Log("Failed to parse error response");
            errorResponse = null;
        }
        Logger.Log(
            $"Error response from {response.RequestMessage!.Method} {response.RequestMessage!.RequestUri}: {errorResponse}"
        );
        return errorResponse!.Errors?.Values.First().First() ?? ResponseToString(response);
    }

    private static string ResponseToString(HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Constants.UnauthorisedMessage,
            HttpStatusCode.Forbidden => Constants.ForbiddenMessage,
            _ => Constants.DefaultMessage
        };
    }

    public static async Task<HttpResponseMessage> LoggedRequest(
        Func<Task<HttpResponseMessage>> request
    )
    {
        var response = await request.Invoke();
        Logger.Log(
            $"Response to {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}: {response}"
        );
        return response;
    }
}
