using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using ImageMagick;
using LapisBot_Renewed.Collections;
using System.DrawingCore;
using System.Xml.Linq;
using Flurl;
using System.Diagnostics;

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

        public string Post(string path, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            
            //string postableContent = ToUriQueryString(content);
            return PostCore(new UriBuilder(_baseUrl) { Path = path }.Uri.AbsoluteUri, JsonConvert.SerializeObject(content));
        }

        public string ImageToBase64(string fileFullName)
        {
            Image image = UrlToImage(fileFullName);
            Bitmap bitmap = new Bitmap(image);
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.DrawingCore.Imaging.ImageFormat.Jpeg);
            byte[] bytes = new byte[stream.Length]; stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length); stream.Close();
            return Convert.ToBase64String(bytes);
        }

        public static string StreamToBase64(MemoryStream stream)
        {
            byte[] bytes = new byte[stream.Length]; stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length); stream.Close();
            return Convert.ToBase64String(bytes);
        }

        public string ImageToPng(string fileFullName, string fatherPath, string name)
        {
            File.Delete(fatherPath + @"/" + name);
            Image image = UrlToImage(fileFullName);
            Bitmap bitmap = new Bitmap(image);
            bitmap.Save(fatherPath + @"/" + name, System.DrawingCore.Imaging.ImageFormat.Png);
            return fatherPath + @"/" + name;
        }

        public string BytesToPng(string fatherPath, string name, byte[] bytes)
        {
            WebClient client = new WebClient();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Image outputImg = Image.FromStream(stream);
                File.Delete(fatherPath + @"/" + name);
                Bitmap bitmap = new Bitmap(outputImg);
                bitmap.Save(fatherPath + @"/" + name, System.DrawingCore.Imaging.ImageFormat.Png);
                return fatherPath + @"/" + name;
            }
        }

        private Image UrlToImage(string url)
        {
            WebClient client = new WebClient();
            byte[] bytes = client.DownloadData(url);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Image outputImg = Image.FromStream(stream);
                return outputImg;
            }
        }

        private string PostCore(string url, string content)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0;)";

            byte[] data = Encoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            string result;
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private string GetCore(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0;)";

            var response = (HttpWebResponse)request.GetResponse();

            string result;
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}
