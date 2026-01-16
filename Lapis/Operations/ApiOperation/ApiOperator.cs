using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Lapis.Operations.ImageOperation;
using Newtonsoft.Json;

namespace Lapis.Operations.ApiOperation;

public class ApiOperator
{
    public static ApiOperator Instance;

    public RequestResult Get(string baseUrl, string path)
    {
        return GetCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri, 10);
    }

    public RequestResult Get(string url)
    {
        return GetCore(url, 10);
    }

    public RequestResult Get(string baseUrl, string path, int timeOut)
    {
        return GetCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri, timeOut);
    }

    public RequestResult Get(string baseUrl, string path, Dictionary<string, string> parameters, int timeOut)
    {
        var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return GetCore(new UriBuilder(baseUrl) { Path = path, Query = query }.Uri.AbsoluteUri, timeOut);
    }

    public RequestResult Delete(string baseUrl, string path, int timeOut)
    {
        return DeleteCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri, timeOut);
    }

    public RequestResult Delete(string baseUrl, string path, Dictionary<string, string> parameters, int timeOut)
    {
        var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return DeleteCore(new UriBuilder(baseUrl) { Path = path, Query = query }.Uri.AbsoluteUri, timeOut);
    }

    public RequestResult Get(string url, int timeOut)
    {
        return GetCore(url, timeOut);
    }

    public RequestResult Post(string url, object content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(url,
            JsonConvert.SerializeObject(content), 10);
    }

    public RequestResult Post(string baseUrl, string path, object content, AuthenticationHeaderValue authorization)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), 1200, [], authorization);
    }

    public RequestResult Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), 10, headers);
    }

    public RequestResult Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers,
        int timeOut)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), timeOut, headers);
    }

    public RequestResult Post(string baseUrl, string path, object content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), 10);
    }

    public RequestResult Post(string baseUrl, string path, object content, int timeOut)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), timeOut);
    }

    public Image UrlToImage(string url)
    {
        var client = new HttpClient();
        var bytes = client.GetByteArrayAsync(url).Result;
        using var stream = new MemoryStream(bytes);
        var outputImg = new Image(stream);
        client.Dispose();
        return outputImg;
    }

    private RequestResult DeleteCore(string url, int timeOut)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var httpResponse = httpClient.DeleteAsync(new Uri(url)).Result;

        if (httpResponse.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {httpResponse.StatusCode}", null,
                httpResponse.StatusCode);

        var reader = new StreamReader(httpResponse.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponse.Dispose();
        reader.Dispose();

        return new RequestResult(result, httpResponse.StatusCode);
    }

    private RequestResult PostCore(string url, string content, int timeOut,
        KeyValuePair<string, string>[] headers = null, AuthenticationHeaderValue authorization = null)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        if (authorization != null)
            httpClient.DefaultRequestHeaders.Authorization = authorization;

        if (headers != null)
            foreach (var header in headers)
                stringContent.Headers.Add(header.Key, header.Value);

        var httpResponse = httpClient.PostAsync(new Uri(url), stringContent).Result;

        var reader = new StreamReader(httpResponse.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponse.Dispose();
        reader.Dispose();

        return new RequestResult(result, httpResponse.StatusCode);
    }

    private RequestResult GetCore(string url, int timeOut)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var httpResponse = httpClient.GetAsync(new Uri(url)).Result;

        if (httpResponse.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {httpResponse.StatusCode}", null,
                httpResponse.StatusCode);

        var reader = new StreamReader(httpResponse.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponse.Dispose();
        reader.Dispose();

        return new RequestResult(result, httpResponse.StatusCode);
    }

    public class RequestResult(string result, HttpStatusCode statusCode)
    {
        public string Result = result;
        public HttpStatusCode StatusCode = statusCode;
    }
}