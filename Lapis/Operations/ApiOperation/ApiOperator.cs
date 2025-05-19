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
            JsonConvert.SerializeObject(content), headers, 10);
    }

    public string Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers, int timeOut)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), headers, timeOut);
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

    private string PostCore(string url, string content, int timeOut)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var httpResponseMessage = httpClient.PostAsync(new Uri(url), stringContent).Result;

        var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponseMessage.Dispose();
        reader.Dispose();

        return result;
    }

    private string PostCore(string url, string content, KeyValuePair<string, string>[] headers, int timeOut)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        foreach (var header in headers) stringContent.Headers.Add(header.Key, header.Value);

        var httpResponseMessage = httpClient.PostAsync(new Uri(url), stringContent).Result;

        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException();

        var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponseMessage.Dispose();
        reader.Dispose();

        return result;
    }

    private string GetCore(string url, int timeOut)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeOut)
        };
        var httpResponseMessage = httpClient.GetAsync(new Uri(url)).Result;

        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException();

        var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponseMessage.Dispose();
        reader.Dispose();

        return result;
    }
}