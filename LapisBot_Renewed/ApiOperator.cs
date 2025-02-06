using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using ImageMagick;
using LapisBot_Renewed.Collections;
using System.Diagnostics;
using System.Net.Http;

namespace LapisBot_Renewed
{
    public class ApiOperator
    {
        private readonly string _baseUrl;

        public ApiOperator(string baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException("baseUrl");
            }

            _baseUrl = baseUrl;
        }

        public string Get(string url)
        {
            /*string query = content == null ? string.Empty : ToUriQueryString(content);
            var url = new UriBuilder(_baseUrl) { Path = path, Query = query }.Uri.AbsoluteUri;*/
            return GetCore(url);
        }

        public string Post(string path, object content, bool withBaseUrl)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (withBaseUrl)
                //string postableContent = ToUriQueryString(content);
                return PostCore(new UriBuilder(_baseUrl) { Path = path }.Uri.AbsoluteUri,
                    JsonConvert.SerializeObject(content));
            else
                return PostCore(path, JsonConvert.SerializeObject(content));
        }
        
        public MagickImage UrlToImage(string url)
        {
            var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(url).Result;
            var stream = new MemoryStream(bytes);
            var outputImg = new MagickImage(stream)
            {
                Format = MagickFormat.Png
            };
            stream.Dispose();
            client.Dispose();
            return outputImg;
        }

        private string PostCore(string url, string content)
        {
            var httpClient = new HttpClient();
            var stringContent = new StringContent(content);
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
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
}
