using Tea;

namespace Alipay.EasySDK.Kernel;

public class Config : TeaModel
{
    [NameInMap("protocol")]
    [Validation(Required = true)]
    public string Protocol { get; set; } = "https";

    [NameInMap("gatewayHost")]
    [Validation(Required = true)]
    public string GatewayHost { get; set; } = "openapi.alipay.com";

    [NameInMap("appId")]
    [Validation(Required = true)]
    public string AppId { get; set; }

    [NameInMap("signType")]
    [Validation(Required = true)]
    public string SignType { get; set; } = "RSA2";

    [NameInMap("alipayPublicKey")]
    [Validation(Required = true)]
    public string AlipayPublicKey { get; set; }

    [NameInMap("merchantPrivateKey")]
    [Validation(Required = true)]
    public string MerchantPrivateKey { get; set; }

    [NameInMap("merchantCertPath")]
    [Validation(Required = true)]
    public string MerchantCertPath { get; set; }

    [NameInMap("alipayCertPath")]
    [Validation(Required = true)]
    public string AlipayCertPath { get; set; }

    [NameInMap("alipayRootCertPath")]
    [Validation(Required = true)]
    public string AlipayRootCertPath { get; set; }

    [NameInMap("notifyUrl")]
    [Validation(Required = true)]
    public string NotifyUrl { get; set; }

    [NameInMap("encryptKey")]
    [Validation(Required = true)]
    public string EncryptKey { get; set; }

    [NameInMap("httpProxy")]
    [Validation(Required = true)]
    public string HttpProxy { get; set; }

    [NameInMap("ignoreSSL")]
    [Validation(Required = true)]
    public string IgnoreSSL { get; set; }
}