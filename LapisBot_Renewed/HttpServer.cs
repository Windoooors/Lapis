using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

namespace LapisBot_Renewed
{
	public class HttpServer
	{
        HttpListener http = new HttpListener();
        Task listen;
        string request = "";

        public void Initialize()
		{
            http.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            http.Prefixes.Add("http://127.0.0.1:19198/");
            http.Start();
            listen = new Task(httpListen);
            listen.Start();
        }

        void httpListen()
        {
            while (http.IsListening)
            {
                var context = http.GetContext();
                Console.Write(context.Request.HttpMethod);
                StreamReader reader = new StreamReader(context.Request.InputStream);
                var data = reader.ReadToEnd();
                Console.Write(data);
                request = data;
                var response = new Response();
                response.Get(request);
                context.Response.StatusCode = 200;
                StreamWriter stream = new StreamWriter(context.Response.OutputStream);
                stream.WriteLine("Hello!!!");
                stream.Close();
                context.Response.Close();
            }
            Console.Write("exit listen");
        }

        public class Response
        {
            bool IsJoined;
            string Code;

            public void Get(string request)
            {
                try
                {
                    var id = request.Split("=")[1];
                    
                    Code = "200";
                }
                catch
                {
                    Code = "501";
                }
            }
        }
    }
}

