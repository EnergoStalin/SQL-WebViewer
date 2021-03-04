using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backend.Http
{
    class HttpServer<ApiProvider>
    {
        private HttpListener listener;
        private Thread thread;
        public void Reciever()
        {
            while (true)
            {
                var ctx = listener.GetContext();

                if(ctx.Request.Url.LocalPath.Contains(".."))
                {
                    ctx.Response.StatusCode = 502;
                    ctx.Response.Close();
                    continue;
                }

                if (ctx.Request.Url.LocalPath == "/" || ctx.Request.Url.LocalPath == String.Empty)
                {
                    SendFile(ctx.Response, "index.html");
                }
                else if(ctx.Request.Url.LocalPath.StartsWith("/api/"))
                {
                    ApiHandler(ctx.Response, ctx.Request);
                }
                else
                {
                    SendFile(ctx.Response, ctx.Request.Url.LocalPath.Trim(new char[] { '.', '/' }));
                }
            }
        }
        private void SendFile(HttpListenerResponse res, string path)
        {
            try
            {
                using (var sr = File.OpenText(path))
                {
                    string text = sr.ReadToEnd();
                    res.ContentEncoding = Encoding.UTF8;
                    res.StatusCode = 200;
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    res.ContentLength64 = bytes.Length;
                    res.OutputStream.Write(bytes,0,bytes.Length);
                    res.ContentType = "text/" + Path.GetExtension(path).Substring(1);
                    res.Close();
                }
            }
            catch (Exception)
            {
                res.StatusCode = 404;
                res.Close();
            }
        }
        private void ApiHandler(HttpListenerResponse res, HttpListenerRequest req)
        {
            foreach (var mem in typeof(ApiProvider).GetMethods())
            {
                if(mem.Name == req.Url.LocalPath.Substring(5))
                {
                    mem.Invoke(null, new object[] { res, req });
                }
            }
        }
        public HttpServer(string prefix)
        {
            listener = new HttpListener()
            {
                Prefixes =
                {
                    prefix
                }
            };

            listener.Start();

            thread = new Thread(Reciever);
            thread.Start();
        }
        public void Dispose() { thread?.Abort(); listener?.Stop(); }
        ~HttpServer() { Dispose(); }
    }
}
