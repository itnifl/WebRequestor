using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using splashmodels;

namespace WebRequestor {
   public class ReadWebResponse {
      public SendWebRequest SWR {get; set;}
      private Status __status;
      private string __errorStatusMessage;
      private string __apiStatusMessage;
      public string XmlFirstRead {get; set;}
      public string XmlErrorRead {get; set;}
      private string __fetchAttributeOrTag;
      private string __fetchErrorAttributeOrTag;
      private string __fetchErrorCodeAttributeOrTag;

      /// <summary>
      /// For reading XML or JSON data that is returned from SendWebRequest. If not used to read XML, then xmlFirstRead and xmlErrorRead do not need to be populated.
      /// </summary>
      /// <param name="swr">The SendWebRequest object that we will use</param>
      /// <param name="status">The Status enum status code before we start reading. Parameter is nullable and set to 'working' of parameter is null.</param>
      /// <param name="errorStatusMessage">String message of error before we start reading. Parameter is nullable and set to 'None' of parameter is null.</param>
      /// <param name="apiStatusMessage">String message of api error before we start reading. Parameter is nullable and set to 'None' of parameter is null.</param>            
      public ReadWebResponse(ref SendWebRequest swr, Status? status, string errorStatusMessage = "", string apiStatusMessage = "") {
         if (swr.Equals(null)) throw new ArgumentException("Parameter swr cannot be null or empty", "swr");
         this.SWR = swr;
         this.__status = status.HasValue ? status.Value : Status.working;
         this.__errorStatusMessage = String.IsNullOrEmpty(errorStatusMessage) ? "None" : errorStatusMessage;
         this.__apiStatusMessage = String.IsNullOrEmpty(apiStatusMessage) ? "None" : apiStatusMessage;         
      }

      /// <summary>
      /// Reads text and returns text. 
      /// </summary>
      /// <param name="verb">The verb to execute the SendWebRequest with; POST, GET, PUT or DELETE.</param>
      /// <returns>A dictionary with they keys errorMessage, status and answerMessage where the text is returned.</returns>
      public Dictionary<string, string> executeTextRead(string verb) {
         Dictionary<string, string> returnAttributes = new Dictionary<string, string>();
         var statusArray = Enum.GetValues(typeof(Status));
         string response = "{}";
         try {
            response = readTextReponse(verb);
            __status = Status.done;
            __errorStatusMessage = "None";
         } catch(Exception e) {
            __errorStatusMessage = e.Message + " - "  + e.InnerException;
            __status = Status.error;
         }
         returnAttributes.Add("errorMessage", __errorStatusMessage);
         returnAttributes.Add("status", __status.ToString());
         returnAttributes.Add("answerMessage", response);
         return returnAttributes;
      }

      /// <summary>
      /// Reads text response from the given SendWebRequest object passed to the constructor of this object using verb passed as an argument to this method and replies with JObjects in dictionaries.
      /// </summary>
      /// <param name="verb">The verb to execute the SendWebRequest with; POST, GET, PUT or DELETE.</param>
      /// <returns>A dictionary with the keys error and answerMessage having corresponding JObjects as values. Error is a jObject with the properties errorMessage and status.</returns>
      public Dictionary<string, JObject> executeJSONRead(string verb) {
         Dictionary<string, JObject> returnAttributes = new Dictionary<string, JObject>();
         var statusArray = Enum.GetValues(typeof(Status));
         JObject response = new JObject();
         try {
            response = JsonConvert.DeserializeObject<JObject>(readTextReponse(verb));
            __status = Status.done;
            __errorStatusMessage = "None";
         }
         catch (Exception e) {
            __errorStatusMessage = e.Message + " - " + e.InnerException;
            __status = Status.error;
         }
         dynamic errorObject = new JObject();
         errorObject.errorMessage = __errorStatusMessage;
         errorObject.status = __status.ToString();
         returnAttributes.Add("error", (JObject)errorObject);
         returnAttributes.Add("answerMessage", response);
         return returnAttributes;
      }

      /// <summary>
      /// Reads XML for attributes as specified by the arguments.
      /// </summary>
      /// <param name="fetchAttribute">The attribute we are looking to fetch data from.</param>
      /// <param name="fetchErrorAttribute">The attribute that might contain error information.</param>
      /// <param name="fetchErrorCodeAttribute">The attribute that might contain the error code.</param>
      /// <param name="xmlFirstRead">The XML tag that has the attribute we are looking for.</param>
      /// <param name="xmlErrorRead">The XML tag that has the error information if something fails.</param>
      /// <returns>A dictionary with the keys errorMessage, status and answerMessage.</returns>
      public Dictionary<string, string> executeXMLRead(string fetchAttribute, string fetchErrorAttribute, string fetchErrorCodeAttribute, string xmlFirstRead, string xmlErrorRead) {
         if (String.IsNullOrEmpty(fetchAttribute)) throw new ArgumentException("Parameter fetchAttribute cannot be null or empty", "fetchAttribute");
         this.__fetchAttributeOrTag = fetchAttribute;
         if (String.IsNullOrEmpty(fetchErrorAttribute)) throw new ArgumentException("Parameter fetchErrorAttribute cannot be null or empty", "fetchErrorAttribute");
         this.__fetchErrorAttributeOrTag = fetchErrorAttribute;
         if (String.IsNullOrEmpty(fetchErrorCodeAttribute)) throw new ArgumentException("Parameter fetchErrorCodeAttribute cannot be null or empty", "fetchErrorCodeAttribute");
         this.__fetchErrorCodeAttributeOrTag = fetchErrorCodeAttribute;
         if (String.IsNullOrEmpty(xmlFirstRead)) throw new ArgumentException("Parameter xmlFirstRead cannot be null or empty", "xmlFirstRead");
         this.XmlFirstRead = xmlFirstRead;
         if (String.IsNullOrEmpty(xmlErrorRead)) throw new ArgumentException("Parameter xmlErrorRead cannot be null or empty", "xmlErrorRead");
         this.XmlErrorRead = xmlErrorRead;
         
         Dictionary<string, string> returnAttributes = new Dictionary<string, string>();
         var statusArray = Enum.GetValues(typeof(Status));
         string responseText = readTextReponse("POST");

         try {
            string tmpFilename = TempFileHandler.CreateTmpFile();
            TempFileHandler.UpdateTmpFile(tmpFilename, responseText);
            AttributeCollection attrCollection = new AttributeCollection("");
            try {
               XmlReaderLocal xmlResponseReader = new XmlReaderLocal(tmpFilename);
               attrCollection = xmlResponseReader.ReadFirstNodeAttributes(XmlFirstRead);
               if (attrCollection == null) { //if the object is null, then it was not found and we then search for the error instead:
                  attrCollection = xmlResponseReader.ReadFirstNodeAttributes(XmlErrorRead);
                  __errorStatusMessage = attrCollection.Attributes.ContainsKey(__fetchErrorAttributeOrTag) ? attrCollection.Attributes[__fetchErrorAttributeOrTag] : "Error occured, but error description was not found.";
                  __status = (Status)statusArray.GetValue(6);
                  __apiStatusMessage = attrCollection.Attributes.ContainsKey(__fetchAttributeOrTag) ? attrCollection.Attributes[__fetchAttributeOrTag] : (attrCollection.Attributes.ContainsKey("errorCode") ? attrCollection.Attributes["errorCode"] + " - " + attrCollection.Attributes[__fetchErrorAttributeOrTag] : "No API status was returned");
               } else if (attrCollection.Attributes.ContainsKey(__fetchErrorAttributeOrTag)) {
                  __errorStatusMessage = attrCollection.Attributes.ContainsKey(__fetchErrorAttributeOrTag) ? attrCollection.Attributes[__fetchErrorAttributeOrTag] : "Error occured, but error description was not found.";
                  __status = (Status)statusArray.GetValue(6);
                  __apiStatusMessage = attrCollection.Attributes.ContainsKey(__fetchAttributeOrTag) ? attrCollection.Attributes[__fetchAttributeOrTag] : (attrCollection.Attributes.ContainsKey("errorCode") ? attrCollection.Attributes["errorCode"] + " - " + attrCollection.Attributes[__fetchErrorAttributeOrTag] : "No API status was returned");
               }
            } catch (Exception ex) {
               __status = (Status)statusArray.GetValue(6);
               __errorStatusMessage = "101 in executeXMLRead(): " + ex.Message;
            }
            if (__status.Equals(Status.done)) {
               __apiStatusMessage = attrCollection.Attributes.ContainsKey(__fetchAttributeOrTag) ? attrCollection.Attributes[__fetchAttributeOrTag] : (attrCollection.Attributes.ContainsKey("errorCode") ? attrCollection.Attributes["errorCode"] + " - " + attrCollection.Attributes[__fetchErrorAttributeOrTag] : "No API status was returned");
               TempFileHandler.DeleteTmpFile(tmpFilename);
            }
         } catch (Exception ex) {
            __status = (Status)statusArray.GetValue(6);
            __errorStatusMessage = "102 in executeXMLRead(): " + ex.Message;
         }
         returnAttributes.Add("errorMessage", __errorStatusMessage);
         returnAttributes.Add("status", __status.ToString());
         returnAttributes.Add("answerMessage", __apiStatusMessage);
         return returnAttributes;
      }

      /// <summary>
      /// Read text response from SendWebRequest object.
      /// </summary>
      /// <param name="verb">The verb to execute the SendWebRequest with; POST, GET, PUT or DELETE.</param>
      /// <returns>String response read by this reader.</returns>
      private String readTextReponse(string verb) {
         string responseText = "";
         verb = verb.ToUpper();
         HttpWebResponse swrResponse;
         switch(verb) {
            case "POST":
                swrResponse = SWR.executePost(new Dictionary<string, string>());
                break;
            default:
                return "{error: 'Wrong or not implemented verb passed to readTextReponse method: " + verb + "'}";
         }
         
         Stream receiveStream = swrResponse.GetResponseStream();
         Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
         StreamReader readStream = new StreamReader(receiveStream, encode);

         using (readStream) {
            responseText = readStream.ReadToEnd();
         }
         swrResponse.Close();
         readStream.Close();
         return responseText;
      }
   }
}
