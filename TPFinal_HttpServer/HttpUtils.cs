using System;
using System.IO;

namespace TPHttpServer
{
    public static class HttpUtils
    {
        public static HttpReq parseHttpRequest(string requestString)
        {
            string httpMethod = "";
            string httpReqUri = "";
            string httpVersion = "";
            using (StringReader reader = new StringReader(requestString))
            {
                // Parse Request Line
                string requestLine = reader.ReadLine();
                if (requestLine != null)
                {
                    string[] parts = requestLine.Split(' ');
                    if (parts.Length >= 3)
                    {

                        httpMethod = parts[0];

                        httpReqUri = parts[1];

                        httpVersion = parts[2];
                    }
                }
            }

            return new HttpReq(httpMethod, httpReqUri, httpVersion);
        }
        
        
    }

    public class HttpReq
    {
        public string httpMethod { get; }
        public string httpUri { get; }
        public string httpVersion { get; }
        public HttpReq(string method, string uri, string version)
        {
            httpMethod = method;
            httpUri = uri;
            httpVersion = version;
        }

    }

    public class HttpResp
    {
        public string contentType = "text/html";
        public int statusCode = 200;
        public static string httpOk = "HTTP/1.1 200 OK";
        public static string http404 = "HTTP/1.1 404 Not Found";
        public int contentLength = 0;

        public string GetResponseHeaders()
        {
            string firstLine = "";
            switch (statusCode)
            {
                case 200:
                    firstLine = httpOk;
                    break;
                case 404:
                    firstLine = http404;
                    break;
            }
            return $"{firstLine}\r\nContent-Length: {contentLength}\r\nContent-Type: {contentType}\r\n\r\n";
        }
    }
}