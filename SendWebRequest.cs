using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebRequestor {
   public class SendWebRequest {
      private string __apiUrl;
      public string ApiRoute { get; set; }
      public string StringData { get; set;  }
      public Dictionary<string, string> Headers {get; set;}
      public string ContentType {get; set;}

      /// <summary>
      /// Implements methods that create and execute web requests and the answer as returns HttpWebResponse</summary>
      /// <param name="apiUrl"> URL too the API ex: http://this.api.com</param>      
      /// <param name="apiRoute"> Route to use in the API ex: api/cusomer</param>
      /// <param name="stringData"> Data payload to send</param>
      public SendWebRequest(string apiUrl, string apiRoute, string stringData) {
         __apiUrl = apiUrl[apiUrl.Length - 1] == '/' ? apiUrl.TrimEnd('/') : apiUrl;
         this.StringData = stringData;
         this.ApiRoute = (apiRoute[0] == '/' ? apiRoute.TrimStart('/') : apiRoute);
         this.Headers = new Dictionary<string, string>();
         ContentType = "application/xml";
      }

      public HttpWebResponse executePost(Dictionary<string, string> ourHeaders) {
         CookieContainer cookieJar = new CookieContainer();
         HttpWebRequest firstRequest = (HttpWebRequest)WebRequest.Create(__apiUrl + "/" + this.ApiRoute);
         if (ourHeaders != null && ourHeaders.Count > 0) {
            this.Headers = this.Headers.Concat(ourHeaders).ToDictionary(k => k.Key, v => v.Value);
         }
         ASCIIEncoding encoding = new ASCIIEncoding();
         byte[] data = encoding.GetBytes(this.StringData);

         firstRequest.CookieContainer = cookieJar;
         firstRequest.KeepAlive = true;
         firstRequest.Method = "POST";
         firstRequest.ContentType = ContentType;
         firstRequest.ContentLength = data.Length;
         firstRequest.Accept = "*/*";
         firstRequest.Referer = "https://" + __apiUrl.Split('/')[2] + "/" + this.ApiRoute;
         firstRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.114 Safari/537.36";
         firstRequest.Host = __apiUrl.Split('/')[2];
         if (this.Headers.Count > 0) {
            foreach(KeyValuePair<string, string> header in this.Headers) {
               firstRequest.Headers[header.Key] = header.Value;
            }
         }

         System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors) {
                                       return true; // **** Always accept SSL Certificate
                                    };

         Stream newStream = firstRequest.GetRequestStream();
         newStream.Write(data, 0, data.Length);
         newStream.Close();
         return (HttpWebResponse)firstRequest.GetResponse();
      }
   }
}
