using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LapisBot.Operations.ImageOperation;
using Newtonsoft.Json;

namespace LapisBot.Operations.ApiOperation;

public class ApiOperator
{
    public static ApiOperator Instance;

    public string Get(string baseUrl, string path)
    {
        return GetCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri);
    }

    public string Get(string url)
    {
        return GetCore(url);
    }

    public string Post(string url, object content)
    {
        if (content == null) throw new ArgumentNullException("content");

        //string postableContent = ToUriQueryString(content);
        return PostCore(url,
            JsonConvert.SerializeObject(content));
    }

    public string Post(string baseUrl, string path, object content, KeyValuePair<string, string>[] headers)
    {
        if (content == null) throw new ArgumentNullException("content");

        //string postableContent = ToUriQueryString(content);
        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content), headers);
    }

    public string Post(string baseUrl, string path, object content)
    {
        if (content == null) throw new ArgumentNullException("content");

        //string postableContent = ToUriQueryString(content);
        return PostCore(new UriBuilder(baseUrl) { Path = path }.Uri.AbsoluteUri,
            JsonConvert.SerializeObject(content));
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

    private string PostCore(string url, string content)
    {
        var httpClient = new HttpClient();
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

    private string PostCore(string url, string content, KeyValuePair<string, string>[] headers)
    {
        var httpClient = new HttpClient();
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        foreach (var header in headers) stringContent.Headers.Add(header.Key, header.Value);

        var httpResponseMessage = httpClient.PostAsync(new Uri(url), stringContent).Result;

        var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponseMessage.Dispose();
        reader.Dispose();

        return result;
    }

    private string GetCore(string url)
    {
        var httpClient = new HttpClient();
        var httpResponseMessage = httpClient.GetAsync(new Uri(url)).Result;

        var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
        var result = reader.ReadToEnd();

        httpClient.Dispose();
        httpResponseMessage.Dispose();
        reader.Dispose();

        return result;
    }
}