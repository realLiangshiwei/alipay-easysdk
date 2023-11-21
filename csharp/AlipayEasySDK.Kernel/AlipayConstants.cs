using System.Text;

namespace Alipay.EasySDK.Kernel;

public static class AlipayConstants
{
    public const string PROTOCOL_CONFIG_KEY = "protocol";
    public const string HOST_CONFIG_KEY = "gatewayHost";
    public const string ALIPAY_CERT_PATH_CONFIG_KEY = "alipayCertPath";
    public const string MERCHANT_CERT_PATH_CONFIG_KEY = "merchantCertPath";
    public const string ALIPAY_ROOT_CERT_PATH_CONFIG_KEY = "alipayRootCertPath";
    public const string SIGN_TYPE_CONFIG_KEY = "signType";
    public const string NOTIFY_URL_CONFIG_KEY = "notifyUrl";
    public const string BIZ_CONTENT_FIELD = "biz_content";
    public const string ALIPAY_CERT_SN_FIELD = "alipay_cert_sn";
    public const string SIGN_FIELD = "sign";
    public const string SIGN_TYPE_FIELD = "sign_type";
    public const string BODY_FIELD = "http_body";
    public const string NOTIFY_URL_FIELD = "notify_url";
    public const string METHOD_FIELD = "method";
    public const string RESPONSE_SUFFIX = "_response";
    public const string ERROR_RESPONSE = "error_response";
    public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;
    public const string RSA2 = "RSA2";
    public const string SHA_256_WITH_RSA = "SHA256WithRSA";
    public const string RSA = "RSA";
    public const string GET = "GET";
    public const string POST = "POST";
}