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
            string[] httpUriArr = httpReqUri.Split('?', 2);
            if(httpUriArr.Length > 1)
            {
                return new HttpReq(httpMethod, httpUriArr[0], httpVersion, httpUriArr[1]);
            }
            else
            {
                return new HttpReq(httpMethod, httpUriArr[0], httpVersion, "");
            }
            
        }
        
        
    }

    public class HttpReq
    {
        public string httpMethod { get; }
        public string httpUri { get; }
        public string httpVersion { get; }
        public string httpQuery { get; }
        public HttpReq(string method, string uri, string version, string query)
        {
            httpMethod = method;
            httpUri = uri;
            httpVersion = version;
            httpQuery = query.TrimStart('?').Replace('&','\n');   
        }

    }

    public class HttpResp
    {
        public string contentType = "text/html";
        public int statusCode = 200;
        public static string httpOk = "HTTP/1.1 200 OK";
        public static string http404 = "HTTP/1.1 404 Not Found";
        public int contentLength = 0;

        public string contentEncoding = "gzip";

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
            return $"{firstLine}\r\nContent-Length: {contentLength}\r\nContent-Type: {contentType}\r\nContent-Encoding: {contentEncoding}\r\n\r\n";
        }
    }
}