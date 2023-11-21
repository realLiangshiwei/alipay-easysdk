using System.Text;

namespace Alipay.EasySDK.Kernel.Util;

public static class MultipartUtil
{
    public static byte[] GetEntryBoundary(string boundary) => Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

    public static byte[] GetEndBoundary(string boundary) => Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

    public static byte[] GetTextEntry(string fieldName, string fieldValue)
    {
        string s = "Content-Disposition:form-data;name=\"" + fieldName + "\"\r\nContent-Type:text/plain\r\n\r\n" + fieldValue;
        return AlipayConstants.DEFAULT_CHARSET.GetBytes(s);
    }

    public static byte[] GetFileEntry(string fieldName, string filePath)
    {
        ArgumentValidator.CheckArgument(File.Exists(filePath), Path.GetFullPath(filePath) + "文件不存在");
        ArgumentValidator.CheckArgument(Path.GetFileName(filePath).Contains("."), "文件名必须带上正确的扩展名");
        string s = "Content-Disposition:form-data;name=\"" + fieldName + "\";filename=\"" + Path.GetFileName(filePath) + "\"\r\nContent-Type:application/octet-stream\r\n\r\n";
        return AlipayConstants.DEFAULT_CHARSET.GetBytes(s);
    }

    public static void WriteToStream(Stream stream, byte[] content) => stream.Write(content, 0, content.Length);
}