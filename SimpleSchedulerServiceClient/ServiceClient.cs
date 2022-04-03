﻿using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SimpleSchedulerServiceClient;

public class ServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JwtContainer _jwt;
    private readonly Action _redirectToLogin;
    private readonly ILogger<ServiceClient> _logger;

    public ServiceClient(HttpClient httpClient, JwtContainer jwt, Action redirectToLogin,
        ILogger<ServiceClient> logger)
    {
        _httpClient = httpClient;
        _jwt = jwt;
        _redirectToLogin = redirectToLogin;
        _logger = logger;
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                _jwt.Token);
        }
        try
        {
            _logger.LogDebug("Token: {token}", _jwt?.Token);
            _logger.LogDebug("URL: {baseAddress} | {requestUri}", _httpClient.BaseAddress, requestUri);
            _logger.LogDebug("Request: {request}", request);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUri, request);
            _logger.LogDebug("Status Code: {statusCode}", response.StatusCode);
            _logger.LogDebug("Reason Phrase: {message}", response.ReasonPhrase);
            if (response.IsSuccessStatusCode)
            {
                return (null, await response.Content.ReadFromJsonAsync<TReply>());
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _redirectToLogin();
                return (null, null);
            }
            return (new Error(await response.Content.ReadAsStringAsync()), null);
        }
        catch (Exception ex)
        {
            return (new Error(ex.Message), null);
        }
    }
}
