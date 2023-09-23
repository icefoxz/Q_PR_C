﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AOT.Utl
{
    public static class Http
    {
        private static HttpClient HttpClient { get; } = new HttpClient();

        public static async Task<(bool isSuccess, string content, HttpStatusCode code)> SendStringContentAsync(string baseUrl, HttpMethod method,
            string content = null, string token = null, params (string, string)[] queryParams)
        {
            var httpContent = content != null ? new StringContent(content, Encoding.UTF8, "application/json") : null;
            return await SendAsync(baseUrl, method, httpContent, token, queryParams);
        }
        public static async Task<(bool isSuccess, string content, HttpStatusCode code)> SendAsync(string baseUrl, HttpMethod method,
            HttpContent content = null, string token = null, params (string, string)[] queryParams)
        {
            var query = queryParams is { Length: > 0 }
                ? "?" + string.Join("&",
                    queryParams.Select(qp => $"{Uri.EscapeDataString(qp.Item1)}={Uri.EscapeDataString(qp.Item2)}"))
                : string.Empty;
            var url = $"{baseUrl}{query}";

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (content != null) request.Content = content;

            var client = HttpClient;
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, responseContent, response.StatusCode);
        }
    }
}