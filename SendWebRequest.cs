using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebRequestor {
   public enum RequestType {
      POST,
      GET,
      PUT,
      DELETE
   }
   public class SendWebRequest {
      private string __apiUrl;
      /// <summary>
      /// The route where we are sending to on the destination
      /// </summary>
      public string ApiRoute { get; set; }
      /// <summary>
      /// Payload
      /// </summary>
      public string StringData { get; set;  }
      /// <summary>
      /// Additional headers
      /// </summary>
      public Dictionary<string, string> Headers {get; set;}
      /// <summary>
      /// Data type of payload being sent
      /// </summary>
      public string ContentType {get; set;}
      public RequestType WebRequestType { get; set; } = RequestType.POST;

      /// <summary>
      /// Implements methods that create and execute web requests and the answer as returns HttpWebResponse</summary>
      /// <param name="apiUrl"> URL too the API ex: http://this.api.com</param>      
      /// <param name="apiRoute"> Route to use in the API ex: api/cusomer</param>
      /// <param name="stringData"> Data payload to send</param>
      /// <param name="contentType"> Data type of payload (eg. application/xml)</param>
      public SendWebRequest(string apiUrl, string apiRoute, string stringData, string contentType = "application/xml") {
         __apiUrl = apiUrl[apiUrl.Length - 1] == '/' ? apiUrl.TrimEnd('/') : apiUrl;
         removeNoise__apiUrl();

         this.StringData = stringData;
         this.ApiRoute = (apiRoute[0] == '/' ? apiRoute.TrimStart('/') : apiRoute);
         this.Headers = new Dictionary<string, string>();
         ContentType = contentType;
      }
      private void removeNoise__apiUrl() {
         int index = __apiUrl.IndexOf("http://");
         __apiUrl = (index < 0)
             ? __apiUrl
             : __apiUrl.Remove(index, "http://".Length);

         index = __apiUrl.IndexOf("https://");
         __apiUrl = (index < 0)
             ? __apiUrl
             : __apiUrl.Remove(index, "http://".Length);

      }
      public HttpWebResponse Execute(Dictionary<string, string> ourHeaders, bool useSsl = false) {
         CookieContainer cookieJar = new CookieContainer();
         removeNoise__apiUrl();
         HttpWebRequest firstRequest = (HttpWebRequest)WebRequest.Create((useSsl ? "https://" : "http://") + __apiUrl + "/" + this.ApiRoute);         
         
         if (ourHeaders != null && ourHeaders.Count > 0) {
            this.Headers = this.Headers.Concat(ourHeaders).ToDictionary(k => k.Key, v => v.Value);
         }

         ASCIIEncoding encoding = new ASCIIEncoding();
         byte[] data = encoding.GetBytes(this.StringData);
         int dataLength = data.Length;

         if (this.ContentType.ToLower().Contains("json")) {
            dataLength = StringData.Length;
         }

         firstRequest.CookieContainer = cookieJar;
         firstRequest.KeepAlive = true;
         firstRequest.Method = this.WebRequestType.ToString();
         firstRequest.ContentType = this.ContentType;
         firstRequest.ContentLength = dataLength;
         firstRequest.Accept = "*/*";
         firstRequest.Referer = (useSsl ? "https://" : "http://") + __apiUrl + "/" + this.ApiRoute;
         firstRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.114 Safari/537.36";
         firstRequest.Host = __apiUrl.Contains(':') ? __apiUrl.Split(':')[0] : __apiUrl;
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


         using (Stream newStream = firstRequest.GetRequestStream()) { 
            newStream.ReadTimeout = 8000;
            newStream.WriteTimeout = 8000;
            newStream.Write(data, 0, dataLength);
         }
         
         return (HttpWebResponse)firstRequest.GetResponse();
      }
   }
}
