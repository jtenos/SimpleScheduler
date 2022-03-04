using OneOf;
using SimpleSchedulerBlazor.Client.Errors;
using System.Net;

namespace SimpleSchedulerBlazor.Client;

public class ServiceClient
{
    private readonly HttpClient _httpClient;

    public ServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Posts to the server, and handles 404 as NotFound, and all others as Error.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<OneOf<TReply, Error>> TryPostAsync<TRequest, TReply>(
        string requestUri,
        TRequest request
    )
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUri, request);
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadFromJsonAsync<TReply>())!;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundError(await response.Content.ReadAsStringAsync());
            }
            return new GenericError(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return new GenericError(ex.Message);
        }
    }
}
