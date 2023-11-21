using System.Text;

namespace Alipay.EasySDK.Kernel.Util;

public static class PageUtil
{
    public static string BuildForm(string actionUrl, IDictionary<string, string> parameters) => "<form name=\"punchout_form\" method=\"post\" action=\"" + actionUrl + "\">\n" + PageUtil.BuildHiddenFields(parameters) + "<input type=\"submit\" value=\"立即支付\" style=\"display:none\" >\n</form>\n<script>document.forms[0].submit();</script>";

    private static string BuildHiddenFields(IDictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "";
        var stringBuilder = new StringBuilder();
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            if (parameter.Key != null && parameter.Value != null)
                stringBuilder.Append(PageUtil.BuildHiddenField(parameter.Key, parameter.Value));
        }
        return stringBuilder.ToString();
    }

    private static string BuildHiddenField(string key, string value)
    {
        StringBuilder stringBuilder = new StringBuilder(64);
        stringBuilder.Append("<input type=\"hidden\" name=\"");
        stringBuilder.Append(key);
        stringBuilder.Append("\" value=\"");
        stringBuilder.Append(value.Replace("\"", "&quot;")).Append("\">\n");
        return stringBuilder.ToString();
    }
}