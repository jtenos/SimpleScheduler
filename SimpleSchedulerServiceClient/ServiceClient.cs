using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SimpleSchedulerServiceClient;

public class ServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JwtContainer _jwt;

    public ServiceClient(HttpClient httpClient, JwtContainer jwt)
    {
        _httpClient = httpClient;
        _jwt = jwt;
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
        if (_jwt?.Token is not null)
        {
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            //    Convert.ToBase64String(Encoding.UTF8.GetBytes(_jwt.Token)));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                _jwt.Token);
        }
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
