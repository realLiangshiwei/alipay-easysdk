using Newtonsoft.Json.Linq;

namespace Alipay.EasySDK.Kernel.Util;

public static class DictionaryUtil
{
    public static Dictionary<string, object> ObjToDictionary(Dictionary<string, object> iputObj)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        foreach (string key in iputObj.Keys)
        {
            if (iputObj[key] is JArray)
            {
                List<object> inputList = ((JToken) iputObj[key]).ToObject<List<object>>();
                dictionary.Add(key, ConvertList(inputList));
            }
            else if (iputObj[key] is JObject)
            {
                Dictionary<string, object> iputObj1 = ((JToken) iputObj[key]).ToObject<Dictionary<string, object>>();
                dictionary.Add(key, ObjToDictionary(iputObj1));
            }
            else
                dictionary.Add(key, iputObj[key]);
        }
        return dictionary;
    }

    private static List<object> ConvertList(List<object> inputList)
    {
        List<object> objectList = new List<object>();
        foreach (object input in inputList)
        {
            switch (input)
            {
                case JArray _:
                    List<object> inputList1 = ((JToken) input).ToObject<List<object>>();
                    objectList.Add((object) DictionaryUtil.ConvertList(inputList1));
                    continue;
                case JObject _:
                    Dictionary<string, object> iputObj = ((JToken) input).ToObject<Dictionary<string, object>>();
                    objectList.Add((object) DictionaryUtil.ObjToDictionary(iputObj));
                    continue;
                default:
                    objectList.Add(input);
                    continue;
            }
        }
        return objectList;
    }
}