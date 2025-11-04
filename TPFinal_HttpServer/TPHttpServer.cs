using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GetServerEnv;


namespace TPHttpServer
{
    class HttpServer
    {
        public static string host = "";
        public static int port = -1;
        public static string serve_folder = "";

        private static Logger logger;

        public static async Task StartServer()
        {

            //Creamos IPEndpoint para el socket en localhost
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            //Creamos el listener
            using Socket listener = new(ipEndPoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

            //Bindeamos y arrancamos la escucha
            listener.Bind(ipEndPoint);
            listener.Listen();
            Console.WriteLine("Server a la escucha en http://{0}:{1}", IPAddress.Loopback, port);

            bool runServer = true;

            while (runServer)
            {

                Socket handler = await listener.AcceptAsync();
                

                byte[] buffer = new byte[8192];
                var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var rawRequest = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                if (bytesReceived > 0)
                {
                    HttpReq req = HttpUtils.parseHttpRequest(rawRequest);
                    logger.LogWrite($"Request received:\r\n{rawRequest}\r\nQuery params:\n{req.httpQuery}");
                    
                    Console.WriteLine("Request received:\nMETHOD: {0}\nURI:{1}\nHTTP VERSION:{2}", req.httpMethod, req.httpUri, req.httpVersion);
                    switch (req.httpMethod)
                    {
                        case "GET":
                            //Comportamiento GET

                            Action GetAction = new Action(() => GetRequestHandler(req,handler));
                            Task.Run(GetAction);

                            break;
                        case "POST":
                            //Comportamiento POST
                            Console.WriteLine("POST RECEIVED");
                            //POST ya logeado con LogWrite del request.
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static async Task GetRequestHandler(HttpReq request, Socket handler)
        {
            HttpResp response = new HttpResp();

            //Conseguimos el filepath
            string filePath = Path.Combine(serve_folder, request.httpUri.TrimStart('/'));

            //Si es el root, devolvemos index
            if (string.IsNullOrEmpty(request.httpUri) || request.httpUri == "/")
            {
                filePath = Path.Combine(serve_folder, "index.html");
            }


            // Check si el archivo existe
            if (File.Exists(filePath))
            {

                string fileExtension = Path.GetExtension(filePath).ToLower();

                // Seteamos Content-Type según extension
                switch (fileExtension)
                {
                    case ".html":
                    case ".htm":
                        response.contentType = "text/html";
                        break;
                    case ".css":
                        response.contentType = "text/css";
                        break;
                    case ".js":
                        response.contentType = "application/javascript";
                        break;
                    // Default para otros archivos (imagenes, etc.)
                    default:
                        response.contentType = "application/octet-stream";
                        break;
                }
                response.statusCode = 200;

            }
            else
            {

                // Archivo no encontrado
                response.statusCode = 404;
                response.contentType = "text/html";

                //Cambiamos el filePath para que apunte al html de not-found
                filePath = Path.Combine(serve_folder, "not-found.html");

            }
            // Leemos el archivo
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // Lo comprimimos
            fileBytes = Compressor.Compress(fileBytes);

            response.contentLength = fileBytes.Length;

            byte[] headerBytes = Encoding.UTF8.GetBytes(response.GetResponseHeaders());

            //Armamos el buffer a mandar
            byte[] bytesToSend = new byte[fileBytes.Length + headerBytes.Length];
            System.Buffer.BlockCopy(headerBytes, 0, bytesToSend, 0, headerBytes.Length);
            System.Buffer.BlockCopy(fileBytes, 0, bytesToSend, headerBytes.Length, fileBytes.Length);

            await handler.SendAsync(bytesToSend, 0);
            handler.Close();
        }
        

        public static void Main(string[] args)
        {

            //Obtenemos el env
            ServerEnv env = ServerEnv.ServerEnvInstance;
            port = env.GetPort();
            serve_folder = env.GetServePath();

            logger = Logger.LoggerInstance;

            // Arrancamos el server
            Task server = StartServer();
            server.GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}