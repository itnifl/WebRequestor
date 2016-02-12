# WebRequestor - How To
The code is only coded for POST requests as is.
To use JObject in your code, you will need the namespace Newtonsoft.Json.Linq. Also include the namespace Newtonsoft.Json for further compatibility.
```c#
string stringData = JsonConvert.SerializeObject(someObject);
SendWebRequest swr = new SendWebRequest(ApiAddress, @"/insert",stringData);
ReadWebResponse rwr = new ReadWebResponse(swr);
Dictionary<string, JObject> response = rwr.ExecuteJSONRead(RequestType.POST);
```

*Atle Holm*