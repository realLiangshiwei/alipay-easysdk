using System.Text;
using System.Web;
using Alipay.EasySDK.Kernel.Util;
using Newtonsoft.Json;
using Tea;

namespace Alipay.EasySDK.Kernel;

 public class Client
  {
    private readonly Context context;
    private readonly Dictionary<string, string> optionalTextParams = new();
    private readonly Dictionary<string, object> optionalBizParams = new();

    public Client(Context context) => this.context = context;

    public Client InjectTextParam(string key, string value)
    {
      this.optionalTextParams.Add(key, value);
      return this;
    }

    public Client InjectBizParam(string key, object value)
    {
      this.optionalBizParams.Add(key, value);
      return this;
    }

    public string GetConfig(string key) => this.context.GetConfig(key);

    public bool IsCertMode() => this.context.CertEnvironment != null;

    public string GetTimestamp()
    {
      DateTime dateTime = DateTime.UtcNow;
      dateTime = dateTime.AddHours(8.0);
      return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string Sign(
      Dictionary<string, string> systemParams,
      Dictionary<string, object> bizParams,
      Dictionary<string, string> textParams,
      string privateKey)
    {
      IDictionary<string, string> sortedMap = this.GetSortedMap(systemParams, bizParams, textParams);
      var stringBuilder = new StringBuilder();
      foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) sortedMap)
      {
        if (!string.IsNullOrEmpty(keyValuePair.Key) && !string.IsNullOrEmpty(keyValuePair.Value))
          stringBuilder.Append(keyValuePair.Key).Append("=").Append(keyValuePair.Value).Append("&");
      }
      if (stringBuilder.Length > 0)
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
      return Signer.Sign(stringBuilder.ToString(), privateKey);
    }

    private IDictionary<string, string> GetSortedMap(
      Dictionary<string, string> systemParams,
      Dictionary<string, object> bizParams,
      Dictionary<string, string> textParams)
    {
      this.AddOtherParams(textParams, bizParams);
      IDictionary<string, string> paramters = (IDictionary<string, string>) new SortedDictionary<string, string>((IDictionary<string, string>) systemParams, (IComparer<string>) StringComparer.Ordinal);
      if (bizParams != null && bizParams.Count != 0)
        paramters.Add("biz_content", JsonUtil.ToJsonString((IDictionary<string, object>) bizParams));
      if (textParams != null)
      {
        foreach (KeyValuePair<string, string> textParam in textParams)
          paramters.Add(textParam.Key, textParam.Value);
      }
      this.SetNotifyUrl(paramters);
      return paramters;
    }

    private void SetNotifyUrl(IDictionary<string, string> paramters)
    {
      if (this.GetConfig("notifyUrl") == null || paramters.ContainsKey("notify_url"))
        return;
      paramters.Add("notify_url", this.GetConfig("notifyUrl"));
    }

    public string GetMerchantCertSN() => this.context.CertEnvironment == null ? (string) null : this.context.CertEnvironment.MerchantCertSN;

    public string GetAlipayRootCertSN() => this.context.CertEnvironment == null ? (string) null : this.context.CertEnvironment.RootCertSN;

    public byte[] ToUrlEncodedRequestBody(Dictionary<string, object> bizParams)
    {
      IDictionary<string, string> sortedMap = this.GetSortedMap(new Dictionary<string, string>(), bizParams, (Dictionary<string, string>) null);
      return AlipayConstants.DEFAULT_CHARSET.GetBytes(this.BuildQueryString(sortedMap));
    }

    private string BuildQueryString(IDictionary<string, string> sortedMap)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = 0;
      foreach (KeyValuePair<string, string> sorted in (IEnumerable<KeyValuePair<string, string>>) sortedMap)
      {
        if (!string.IsNullOrEmpty(sorted.Key) && !string.IsNullOrEmpty(sorted.Value))
        {
          stringBuilder.Append(num == 0 ? "" : "&").Append(sorted.Key).Append("=").Append(HttpUtility.UrlEncode(sorted.Value, AlipayConstants.DEFAULT_CHARSET));
          ++num;
        }
      }
      return stringBuilder.ToString();
    }

    public string GetRandomBoundary() => DateTime.Now.Ticks.ToString("X");

    public string ConcatStr(string a, string b) => a + b;

    public Stream ToMultipartRequestBody(
      Dictionary<string, string> textParams,
      Dictionary<string, string> fileParams,
      string boundary)
    {
      MemoryStream multipartRequestBody = new MemoryStream();
      this.AddOtherParams(textParams, (Dictionary<string, object>) null);
      foreach (KeyValuePair<string, string> textParam in textParams)
      {
        if (!string.IsNullOrEmpty(textParam.Key) && !string.IsNullOrEmpty(textParam.Value))
        {
          MultipartUtil.WriteToStream((Stream) multipartRequestBody, MultipartUtil.GetEntryBoundary(boundary));
          MultipartUtil.WriteToStream((Stream) multipartRequestBody, MultipartUtil.GetTextEntry(textParam.Key, textParam.Value));
        }
      }
      foreach (KeyValuePair<string, string> fileParam in fileParams)
      {
        if (!string.IsNullOrEmpty(fileParam.Key) && fileParam.Value != null)
        {
          MultipartUtil.WriteToStream((Stream) multipartRequestBody, MultipartUtil.GetEntryBoundary(boundary));
          MultipartUtil.WriteToStream((Stream) multipartRequestBody, MultipartUtil.GetFileEntry(fileParam.Key, fileParam.Value));
          MultipartUtil.WriteToStream((Stream) multipartRequestBody, File.ReadAllBytes(fileParam.Value));
        }
      }
      MultipartUtil.WriteToStream((Stream) multipartRequestBody, MultipartUtil.GetEndBoundary(boundary));
      multipartRequestBody.Seek(0L, SeekOrigin.Begin);
      return (Stream) multipartRequestBody;
    }

    public Dictionary<string, object> ReadAsJson(TeaResponse response, string method)
    {
      string responseBody = TeaCore.GetResponseBody(response);
      Dictionary<string, object> iputObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
      iputObj.Add("http_body", (object) responseBody);
      iputObj.Add(nameof (method), (object) method);
      return DictionaryUtil.ObjToDictionary(iputObj);
    }

    public async Task<Dictionary<string, object>> ReadAsJsonAsync(
      TeaResponse response,
      string method)
    {
      return this.ReadAsJson(response, method);
    }

    public string GetAlipayCertSN(Dictionary<string, object> respMap) => (string) respMap["alipay_cert_sn"];

    public string ExtractAlipayPublicKey(string alipayCertSN) => this.context.CertEnvironment == null ? (string) null : this.context.CertEnvironment.GetAlipayPublicKey(alipayCertSN);

    public bool Verify(Dictionary<string, object> respMap, string alipayPublicKey)
    {
      string resp = (string) respMap["sign"];
      return Signer.Verify(SignContentExtractor.GetSignSourceData((string) respMap["http_body"], (string) respMap["method"]), resp, alipayPublicKey);
    }

    public Dictionary<string, object> ToRespModel(Dictionary<string, object> respMap)
    {
      string str1 = ((string) respMap["method"]).Replace('.', '_') + "_response";
      string str2 = "error_response";
      foreach (KeyValuePair<string, object> resp in respMap)
      {
        if (str1.Equals(resp.Key))
        {
          Dictionary<string, object> respModel = (Dictionary<string, object>) resp.Value;
          respModel.Add("http_body", respMap["http_body"]);
          return respModel;
        }
      }
      foreach (KeyValuePair<string, object> resp in respMap)
      {
        if (str2.Equals(resp.Key))
        {
          Dictionary<string, object> respModel = (Dictionary<string, object>) resp.Value;
          respModel.Add("http_body", respMap["http_body"]);
          return respModel;
        }
      }
      throw new Exception("响应格式不符合预期，找不到" + str1 + "节点");
    }

    public string GeneratePage(
      string method,
      Dictionary<string, string> systemParams,
      Dictionary<string, object> bizParams,
      Dictionary<string, string> textParams,
      string sign)
    {
      if ("GET".Equals(method))
      {
        IDictionary<string, string> sortedMap = this.GetSortedMap(systemParams, bizParams, textParams);
        sortedMap.Add(nameof (sign), sign);
        return this.GetGatewayServerUrl() + "?" + this.BuildQueryString(sortedMap);
      }
      if (!"POST".Equals(method))
        throw new Exception("_generatePage中method只支持传入GET或POST");
      IDictionary<string, string> sortedMap1 = this.GetSortedMap(systemParams, (Dictionary<string, object>) null, textParams);
      sortedMap1.Add(nameof (sign), sign);
      string actionUrl = this.GetGatewayServerUrl() + "?" + this.BuildQueryString(sortedMap1);
      this.AddOtherParams((Dictionary<string, string>) null, bizParams);
      IDictionary<string, string> parameters = (IDictionary<string, string>) new SortedDictionary<string, string>()
      {
        {
          "biz_content",
          JsonUtil.ToJsonString((IDictionary<string, object>) bizParams)
        }
      };
      return PageUtil.BuildForm(actionUrl, parameters);
    }

    public string GenerateOrderString(
      Dictionary<string, string> systemParams,
      Dictionary<string, object> bizParams,
      Dictionary<string, string> textParams,
      string sign)
    {
      IDictionary<string, string> sortedMap = this.GetSortedMap(systemParams, bizParams, textParams);
      sortedMap.Add(nameof (sign), sign);
      return this.BuildQueryString(sortedMap);
    }

    private string GetGatewayServerUrl() => this.GetConfig("protocol") + "://" + this.GetConfig("gatewayHost") + "/gateway.do";

    public string AesEncrypt(string plainText, string key) => AES.Encrypt(plainText, key);

    public string AesDecrypt(string chiperText, string key) => AES.Decrypt(chiperText, key);

    public bool VerifyParams(Dictionary<string, string> parameters, string alipayPublicKey) => Signer.VerifyParams(parameters, alipayPublicKey);

    public string GetSdkVersion() => this.context.SdkVersion;

    public Dictionary<string, string> SortMap(Dictionary<string, string> input) => input;

    private void AddOtherParams(
      Dictionary<string, string> textParams,
      Dictionary<string, object> bizParams)
    {
      if (textParams != null)
      {
        foreach (KeyValuePair<string, string> optionalTextParam in this.optionalTextParams)
        {
          if (!textParams.ContainsKey(optionalTextParam.Key))
            textParams.Add(optionalTextParam.Key, optionalTextParam.Value);
        }
        this.SetNotifyUrl((IDictionary<string, string>) textParams);
      }
      if (bizParams == null)
        return;
      foreach (KeyValuePair<string, object> optionalBizParam in this.optionalBizParams)
      {
        if (!bizParams.ContainsKey(optionalBizParam.Key))
          bizParams.Add(optionalBizParam.Key, optionalBizParam.Value);
      }
    }
  }