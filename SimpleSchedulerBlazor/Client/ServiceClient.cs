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
    public async Task<(Error?, TReply?)> PostAsync<TRequest, TReply>(
        string requestUri,
        TRequest request
    )
        where TRequest : class
        where TReply : class
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUri, request);
            if (response.IsSuccessStatusCode)
            {
                return (null, await response.Content.ReadFromJsonAsync<TReply>());
            }
            return (new Error(await response.Content.ReadAsStringAsync()), null);
        }
        catch (Exception ex)
        {
            return (new Error(ex.Message), null);
        }
    }
}
