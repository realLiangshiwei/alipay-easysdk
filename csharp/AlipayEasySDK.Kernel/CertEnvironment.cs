using Alipay.EasySDK.Kernel.Util;
using Org.BouncyCastle.X509;

namespace Alipay.EasySDK.Kernel;

public class CertEnvironment
{
    private readonly Dictionary<string, string> CachedAlipayPublicKey = new Dictionary<string, string>();

    public string RootCertContent { get; set; }

    public string RootCertSN { get; set; }

    public string MerchantCertSN { get; set; }

    public CertEnvironment(
        string merchantCertPath,
        string alipayCertPath,
        string alipayRootCertPath)
    {
        if (string.IsNullOrEmpty(merchantCertPath) || string.IsNullOrEmpty(alipayCertPath) || string.IsNullOrEmpty(alipayCertPath))
            throw new Exception("证书参数merchantCertPath、alipayCertPath或alipayRootCertPath设置不完整。");
        this.RootCertContent = File.ReadAllText(alipayRootCertPath);
        this.RootCertSN = AntCertificationUtil.GetRootCertSN(this.RootCertContent);
        this.MerchantCertSN = AntCertificationUtil.GetCertSN(AntCertificationUtil.ParseCert(File.ReadAllText(merchantCertPath)));
        X509Certificate cert = AntCertificationUtil.ParseCert(File.ReadAllText(alipayCertPath));
        this.CachedAlipayPublicKey[AntCertificationUtil.GetCertSN(cert)] = AntCertificationUtil.ExtractPemPublicKeyFromCert(cert);
    }

    public string GetAlipayPublicKey(string sn)
    {
        if (string.IsNullOrEmpty(sn))
            return this.CachedAlipayPublicKey.Values.FirstOrDefault<string>();
        return this.CachedAlipayPublicKey.ContainsKey(sn) ? this.CachedAlipayPublicKey[sn] : throw new Exception("支付宝公钥证书[" + sn + "]已过期，请重新下载最新支付宝公钥证书并替换原证书文件");
    }
}