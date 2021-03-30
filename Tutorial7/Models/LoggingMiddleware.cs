using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tutorial5.Models
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var endPoint = httpContext.Request.Path;
            var queryString = httpContext.Request.QueryString.ToString();

            
            
            var bodyStr = "";
            using (StreamReader reader
                = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = await reader.ReadToEndAsync();
            }

            using (StreamWriter w = File.AppendText(@"C:\Users\hitma\Desktop\apbd\tutorials\Tutorial5\Tutorial5\Data\log.txt"))
            {
                Log(httpMethod, endPoint, queryString, bodyStr,w);
            }

            await _next(httpContext);
        }

        public static void Log(string httpMethod, string endPoint, string queryString, string bodyString, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :Method: {httpMethod}");
            w.WriteLine($"  :EndPoint: {endPoint}");
            w.WriteLine($"  :QueryString: {queryString}");
            w.WriteLine($"  :Body: {bodyString}");
            w.WriteLine("-------------------------------");
        }
    }
}
