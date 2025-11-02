using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GetServerEnv;
using System.Net.Cache;
using Microsoft.VisualBasic;

namespace TPHttpServer
{
    class HttpServer
    {
        public static string host = "";
        public static int port = -1;
        public static string serve_folder = "";

        public static async void GetRequestHandler(HttpReq request, Socket handler)
        {
            //Conseguimos el filepath
            string filePath = Path.Combine(serve_folder, request.httpUri.TrimStart('/'));

            //Si es el root, devolvemos index
            if (string.IsNullOrEmpty(request.httpUri) || request.httpUri == "/")
            {
                filePath = Path.Combine(serve_folder, "index.html");
            }
            HttpResp response = new HttpResp();


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

                response.contentType = contentType;
                response.statusCode = 200;

                // Leemos el archivo y lo enviamos
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] headerBytes = Encoding.UTF8.GetBytes(response.GetResponseHeaders());

                //Armamos el buffer a mandar
                byte[] bytesToSend = new byte[fileBytes.Length + headerBytes.Length];
                System.Buffer.BlockCopy(headerBytes, 0, bytesToSend, 0, headerBytes.Length);
                System.Buffer.BlockCopy(fileBytes, 0, bytesToSend, headerBytes.Length, fileBytes.Length);

                await handler.SendAsync(bytesToSend, 0);
            }
            else
            {
                // Archivo no encontrado
                response.statusCode = 404;
                string notFoundMessage =
                @"<!DOCTYPE html>                      
                <html lang=""es"">
                <head>
                    <meta charset=""UTF-8"">
                </head>
                    <body>
                        <header>
                            <h1>Bienvenido a mi página web</h1>
                        </header>
                        <main>
                            <h1>ERROR 404 - NO ENCONTRADO</h1>
                        </main>
    
                    </body>
                </html>";

                byte[] buffer = Encoding.UTF8.GetBytes(notFoundMessage);
                using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            response.Close();
        }
        public static async Task StartServer()
        {

            //Creamos IPEndpoint para el socket en localhost
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            //Creamos el listener
            using Socket listener = new(ipEndPoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

            //Bindeamos y arrancamos la escucha
            listener.Bind(ipEndPoint);
            listener.Listen();
            Console.WriteLine("Server a la escucha en {0}, en el puerto: {1}", IPAddress.Loopback, port);

            bool runServer = true;

            while (runServer)
            {

                Socket handler = await listener.AcceptAsync();
                

                var buffer = new byte[8192];
                var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var rawRequest = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                if (bytesReceived > 0)
                {
                    HttpReq req = HttpUtils.parseHttpRequest(rawRequest);
                    Console.WriteLine("Request received:\nMETHOD: {0}\nURI:{1}\nHTTP VERSION:{2}", req.httpMethod, req.httpUri, req.httpVersion);

                    switch (req.httpMethod)
                    {
                        case "GET":
                            //Comportamiento GET
                            Action GetAction = new Action(() => GetRequestHandler(req,handler));
                            Task.Run(GetAction);
                            break;
                        case "POST":
                            break;
                        default:
                            break;
                    }
                }
/* 

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
                    Console.WriteLine("Se ha recibido una solicitud HTTP del tipo POST");
                }
                else if ((req.HttpMethod == "GET"))
                {
                    //COMPORTAMIENTO GET
                    Action GetAction = new Action(() => GetRequestHandler(req, resp));
                    Task.Run(GetAction);
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
                resp.Close(); */
            }
        }

        public static void Main(string[] args)
        {

            //Obtenemos el env
            ServerEnv env = ServerEnv.ServerEnvInstance;
            port = env.GetPort();
            serve_folder = env.GetServePath();


            // Arrancamos el server
            Task server = StartServer();
            server.GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}