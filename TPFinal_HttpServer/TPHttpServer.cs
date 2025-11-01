using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using GetServerEnv;
using System.Net.Cache;

namespace TPHttpServer
{
    class HttpServer
    {
        public static HttpListener listener = new HttpListener();
        public static string host = "";
        public static int port = -1;
        public static string serve_folder = "";

        public static void GetRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            //Conseguimos el filepath
            string filePath = Path.Combine(serve_folder, request.RawUrl.TrimStart('/'));

            //Si es el root, devolvemos index
            if (string.IsNullOrEmpty(request.RawUrl) || request.RawUrl == "/")
            {
                filePath = Path.Combine(serve_folder, "index.html");
            }

            // Check si el archivo existe
            if (File.Exists(filePath))
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();
                string contentType;

                // Seteamos Content-Type según extension
                switch (fileExtension)
                {
                    case ".html":
                    case ".htm":
                        contentType = "text/html";
                        break;
                    case ".css":
                        contentType = "text/css";
                        break;
                    case ".js":
                        contentType = "application/javascript";
                        break;
                    // Default para otros archivos (imagenes, etc.)
                    default:
                        contentType = "application/octet-stream";
                        break;
                }

                response.ContentType = contentType;

                // Leemos el archivo y lo enviamos
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentLength64 = buffer.Length;
                using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                // Archivo no encontrado
                response.StatusCode = 404;
                string notFoundMessage = "404 Not Found";
                byte[] buffer = Encoding.UTF8.GetBytes(notFoundMessage);
                response.ContentLength64 = buffer.Length;
                using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            response.Close();
        }
        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST"))
                {
                    //COMPORTAMIENTO POST
                }
                else if ((req.HttpMethod == "GET"))
                {
                    //COMPORTAMIENTO GET
                }
                else
                {
                    Console.WriteLine("Request type not supported: {0}", req.HttpMethod);
                    
                }

                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(String.Format("etc"));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public static void Main(string[] args)
        {

            ServerEnv env = ServerEnv.ServerEnvInstance;
            host = env.GetHost();
            port = env.GetPort();
            serve_folder = env.GetServePath();

            //Usamos la clase UriBuilder para manejar la url
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "http";
            uriBuilder.Host = host;
            uriBuilder.Port = port;

            //Construimos la Uri
            Uri uri = uriBuilder.Uri;

            //Obtenemos la URL de escucha
            string listenUrl = uri.AbsoluteUri;

            //Ponemos el listener a escuchar
            listener.Prefixes.Add(listenUrl);
            listener.Start();
            Console.WriteLine("Server a la escucha en {0}", listenUrl);

            // Manejamos requests
            Task listenTask = HandleIncomingConnections();
            

            // Close the listener
            listener.Close();
            Console.ReadKey();
        }
    }
}