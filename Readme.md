# WebRequestor - How To
The code is only coded for POST and GET requests as is.
To use JObject in your code, you will need the namespace Newtonsoft.Json.Linq. Also include the namespace Newtonsoft.Json for further compatibility.
Remember; 
```c#
using WebRequestor;
```

Example of a POST request:
```c#
string stringData = JsonConvert.SerializeObject(someObject);
SendWebRequest swr = new SendWebRequest(ApiAddress, @"/insert",stringData);
ReadWebResponse rwr = new ReadWebResponse(swr);
Dictionary<string, JObject> response = rwr.ExecuteJSONRead(RequestType.POST);
```

Example of a GET request that deserializes an XML string reply to an object:
```c#
SendWebRequest swr = new SendWebRequest("http://" + address.Authority, "timeinfo/GetSystemTimeZone", String.Empty);
swr.WebRequestType = RequestType.GET;
ReadWebResponse rwr = new ReadWebResponse(swr);
TimeZoneData data = rwr.DeserializeXML<TimeZoneData>(RequestType.GET);
```

*Atle Holm*