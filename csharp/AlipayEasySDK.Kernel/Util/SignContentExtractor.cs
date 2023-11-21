namespace Alipay.EasySDK.Kernel.Util;

public class SignContentExtractor
  {
    public const char LEFT_BRACE = '{';
    public const char RIGHT_BRACE = '}';
    public const char DOUBLE_QUOTES = '"';

    public static string GetSignSourceData(string body, string method)
    {
      string rootNode1 = method.Replace(".", "_") + "_response";
      string rootNode2 = "error_response";
      int indexOfRootNode1 = body.IndexOf(rootNode1, StringComparison.Ordinal);
      int indexOfRootNode2 = body.IndexOf(rootNode2, StringComparison.Ordinal);
      string signSourceData = (string) null;
      if (indexOfRootNode1 > 0)
        signSourceData = SignContentExtractor.ParseSignSourceData(body, rootNode1, indexOfRootNode1);
      else if (indexOfRootNode2 > 0)
        signSourceData = SignContentExtractor.ParseSignSourceData(body, rootNode2, indexOfRootNode2);
      return signSourceData;
    }

    private static string ParseSignSourceData(string body, string rootNode, int indexOfRootNode)
    {
      int begin = indexOfRootNode + rootNode.Length + 2;
      if (body.IndexOf("\"sign\"", StringComparison.Ordinal) < 0)
        return (string) null;
      SignContentExtractor.SignSourceData signContent = SignContentExtractor.ExtractSignContent(body, begin);
      if (body.LastIndexOf(rootNode, StringComparison.Ordinal) > signContent.EndIndex)
        throw new Exception("检测到响应报文中有重复的" + rootNode + "，验签失败。");
      return signContent.SourceData;
    }

    private static SignContentExtractor.SignSourceData ExtractSignContent(string str, int begin)
    {
      if (str == null)
        return (SignContentExtractor.SignSourceData) null;
      int beginPosition = SignContentExtractor.ExtractBeginPosition(str, begin);
      if (beginPosition >= str.Length)
        return (SignContentExtractor.SignSourceData) null;
      int endPosition = SignContentExtractor.ExtractEndPosition(str, beginPosition);
      return new SignContentExtractor.SignSourceData()
      {
        SourceData = str.Substring(beginPosition, endPosition - beginPosition),
        BeginIndex = beginPosition,
        EndIndex = endPosition
      };
    }

    private static int ExtractBeginPosition(string responseString, int begin)
    {
      int index = begin;
      while (index < responseString.Length && responseString[index] != '{' && responseString[index] != '"')
        ++index;
      return index;
    }

    private static int ExtractEndPosition(string responseString, int beginPosition) => responseString[beginPosition] == '{' ? SignContentExtractor.ExtractJsonObjectEndPosition(responseString, beginPosition) : SignContentExtractor.ExtractJsonBase64ValueEndPosition(responseString, beginPosition);

    private static int ExtractJsonBase64ValueEndPosition(string responseString, int beginPosition)
    {
      for (int index = beginPosition; index < responseString.Length; ++index)
      {
        if (responseString[index] == '"' && index != beginPosition)
          return index + 1;
      }
      return responseString.Length;
    }

    private static int ExtractJsonObjectEndPosition(string responseString, int beginPosition)
    {
      LinkedList<char> linkedList = new LinkedList<char>();
      bool flag = false;
      int num = 0;
      for (int index = beginPosition; index < responseString.Length; ++index)
      {
        char ch = responseString[index];
        if (ch == '"' && num % 2 == 0)
          flag = !flag;
        else if (ch == '{' && !flag)
          linkedList.AddLast('{');
        else if (ch == '}' && !flag)
        {
          linkedList.RemoveLast();
          if (linkedList.Count == 0)
            return index + 1;
        }
        if (ch == '\\')
          ++num;
        else
          num = 0;
      }
      return responseString.Length;
    }

    public class SignSourceData
    {
      public string SourceData { get; set; }

      public int BeginIndex { get; set; }

      public int EndIndex { get; set; }
    }
  }