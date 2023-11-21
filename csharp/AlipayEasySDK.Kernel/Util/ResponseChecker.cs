using System.Reflection;
using Tea;

namespace Alipay.EasySDK.Kernel.Util;

public class ResponseChecker
{
    public const string SUB_CODE_FIELD_NAME = "SubCode";

    public static bool Success(TeaModel response)
    {
        PropertyInfo property = response.GetType().GetProperty("SubCode");
        return property == (PropertyInfo) null || string.IsNullOrEmpty((string) property.GetValue((object) response));
    }
}