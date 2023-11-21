namespace Alipay.EasySDK.Kernel.Util;

public static class ArgumentValidator
{
    public static void CheckArgument(bool expression, string errorMessage)
    {
        if (!expression)
            throw new Exception(errorMessage);
    }

    public static void CheckNotNull(object value, string errorMessage)
    {
        if (value == null)
            throw new Exception(errorMessage);
    }

    public static void EnsureNull(object value, string errorMessage)
    {
        if (value != null)
            throw new Exception(errorMessage);
    }
}