using Alipay.EasySDK.Kernel.Util;
using Tea;

namespace Alipay.EasySDK.Kernel;

public class Context
{
    private readonly Dictionary<string, object> config;

    public CertEnvironment CertEnvironment { get; }

    public string SdkVersion { get; set; }

    public Context(Config config, string sdkVersion)
    {
        this.config = config.ToMap();
        this.SdkVersion = sdkVersion;
        ArgumentValidator.CheckArgument("RSA2".Equals(this.GetConfig("signType")), "Alipay Easy SDK只允许使用RSA2签名方式，RSA签名方式由于安全性相比RSA2弱已不再推荐。");
        if (string.IsNullOrEmpty(this.GetConfig("alipayCertPath")))
            return;
        this.CertEnvironment = new CertEnvironment(this.GetConfig("merchantCertPath"), this.GetConfig("alipayCertPath"), this.GetConfig("alipayRootCertPath"));
    }

    public string GetConfig(string key) => (string) this.config[key];
}