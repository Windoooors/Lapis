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
        public string BaseUrl { get { return _baseUrl; } }

        public static string Bash(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();
            return result;
        }

        public ApiOperator(string baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException("baseUrl");
            }

            _baseUrl = baseUrl;
        }

        private static string ToUriQueryString(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return string.Join("&", obj.GetType().GetProperties().Select(prop =>
            {
                object value = prop.GetValue(obj, null);
                return Uri.EscapeDataString(ToUnderScoreCase(prop.Name)) + "=" + Uri.EscapeDataString(value == null ? "null" : value.ToString());
            }).ToArray());
        }

        public string Get(string url)
        {
            /*string query = content == null ? string.Empty : ToUriQueryString(content);
            var url = new UriBuilder(_baseUrl) { Path = path, Query = query }.Uri.AbsoluteUri;*/
            return GetCore(url);
        }

        private static string ToUnderScoreCase(string str)
        {
            var builder = new StringBuilder();

            foreach (char c in str)
            {
                if (char.IsUpper(c))
                {
                    builder.Append('_');
                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            if (builder[0] == '_')
            {
                builder.Remove(0, 1);
            }

            return builder.ToString();
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
            
            return result;
        }

        private string GetCore(string url)
        {
            var httpClient = new HttpClient();
            var httpResponseMessage = httpClient.GetAsync(new Uri(url)).Result;

            var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream(), Encoding.UTF8);
            var result = reader.ReadToEnd();

            return result;
        }
    }
}
