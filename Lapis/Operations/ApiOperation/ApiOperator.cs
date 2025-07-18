using System;
using System.Collections.Generic;
using System.IO;
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

    public string Get(string baseUrl, string path)
    {
        return GetCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri, 10);
    }

    public string Get(string url)
    {
        return GetCore(url, 10);
    }

    public string Get(string baseUrl, string path, int timeOut)
    {
        return GetCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri, timeOut);
    }

    public string Get(string url, int timeOut)
    {
        return GetCore(url, timeOut);
    }

    public string Post(string url, object content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(url,
            JsonConvert.SerializeObject(content), 10);
    }

    public string Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), 10, headers);
    }

    public string Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers, int timeOut)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), timeOut, headers);
    }

    public string Post(string baseUrl, string path, object content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), 10);
    }

    public string Post(string baseUrl, string path, object content, int timeOut)
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
    
    private string PostCore(string url, string content, int timeOut, KeyValuePair<string, string>[] headers = null)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        if (headers != null)
            foreach (var header in headers)
                stringContent.Headers.Add(header.Key, header.Value);

        var httpResponse = httpClient.PostAsync(new Uri(url), stringContent).Result;

        if (httpResponse.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {httpResponse.StatusCode}", null,
                httpResponse.StatusCode);

        var reader = new StreamReader(httpResponse.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponse.Dispose();
        reader.Dispose();

        return result;
    }

    private string GetCore(string url, int timeOut)
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

        return result;
    }
}