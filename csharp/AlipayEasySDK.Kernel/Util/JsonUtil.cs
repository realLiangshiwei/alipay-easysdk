using Newtonsoft.Json;
using Tea;

namespace Alipay.EasySDK.Kernel.Util;

public class JsonUtil
{
    public static string ToJsonString(IDictionary<string, object> input)
    {
        IDictionary<string, object> dictionary = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> keyValuePair in input)
        {
            if (keyValuePair.Value is TeaModel)
                dictionary.Add(keyValuePair.Key, GetTeaModelMap((TeaModel) keyValuePair.Value));
            else
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
        return JsonConvert.SerializeObject(dictionary);
    }

    private static IDictionary<string, object> GetTeaModelMap(TeaModel teaModel)
    {
        IDictionary<string, object> teaModelMap = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> keyValuePair in (IEnumerable<KeyValuePair<string, object>>) teaModel.ToMap())
        {
            if (keyValuePair.Value is TeaModel)
                teaModelMap.Add(keyValuePair.Key, GetTeaModelMap((TeaModel) keyValuePair.Value));
            else
                teaModelMap.Add(keyValuePair.Key, keyValuePair.Value);
        }
        return teaModelMap;
    }
}