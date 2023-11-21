using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Alipay.EasySDK.Kernel.Util;

 public static class AntCertificationUtil
  {
    public static string GetRootCertSN(string rootCertContent)
    {
      string rootCertSn = "";
      try
      {
        foreach (X509Certificate cert in AntCertificationUtil.ReadPemCertChain(rootCertContent))
        {
          if (cert.SigAlgOid.StartsWith("1.2.840.113549.1.1", StringComparison.Ordinal))
          {
            string certSn = AntCertificationUtil.GetCertSN(cert);
            rootCertSn = !string.IsNullOrEmpty(rootCertSn) ? rootCertSn + "_" + certSn : certSn;
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception("提取根证书序列号失败。" + ex.Message);
      }
      return rootCertSn;
    }

    public static X509Certificate ParseCert(string certContent) => new X509CertificateParser().ReadCertificate(Encoding.UTF8.GetBytes(certContent));

    public static string GetCertSN(X509Certificate cert)
    {
      string str = cert.IssuerDN.ToString();
      if (str.StartsWith("CN", StringComparison.Ordinal))
        return AntCertificationUtil.CalculateMd5(str + cert.SerialNumber?.ToString());
      List<string> list = ((IEnumerable<string>) str.Split(',')).ToList<string>();
      list.Reverse();
      return AntCertificationUtil.CalculateMd5(string.Join(",", list.ToArray()) + cert.SerialNumber?.ToString());
    }

    public static bool IsTrusted(string certContent, string rootCertContent) => AntCertificationUtil.VerifyCertChain(AntCertificationUtil.ReadPemCertChain(certContent), AntCertificationUtil.ReadPemCertChain(rootCertContent));

    private static List<X509Certificate> ReadPemCertChain(string cert)
    {
      var collection = new X509CertificateParser().ReadCertificates(Encoding.UTF8.GetBytes(cert));
      List<X509Certificate> x509CertificateList = new List<X509Certificate>();
      foreach (object obj in (IEnumerable) collection)
        x509CertificateList.Add((X509Certificate) obj);
      return x509CertificateList;
    }

    private static bool SortCertChain(List<X509Certificate> certChain)
    {
      Dictionary<X509Name, X509Certificate> subject2CertMap = new Dictionary<X509Name, X509Certificate>();
      Dictionary<X509Name, X509Certificate> issuer2CertMap = new Dictionary<X509Name, X509Certificate>();
      bool flag = false;
      foreach (X509Certificate cert in certChain)
      {
        if (AntCertificationUtil.IsSelfSigned(cert))
        {
          if (flag)
            return false;
          flag = true;
        }
        subject2CertMap[cert.SubjectDN] = cert;
        issuer2CertMap[cert.IssuerDN] = cert;
      }
      List<X509Certificate> x509CertificateList = new List<X509Certificate>();
      X509Certificate current = certChain[0];
      AntCertificationUtil.AddressingUp(subject2CertMap, x509CertificateList, current);
      AntCertificationUtil.AddressingDown(issuer2CertMap, x509CertificateList, current);
      if (certChain.Count != x509CertificateList.Count)
        return false;
      for (int index = 0; index < x509CertificateList.Count; ++index)
        certChain[index] = x509CertificateList[index];
      return true;
    }

    private static bool IsSelfSigned(X509Certificate cert) => cert.SubjectDN.Equivalent(cert.IssuerDN);

    private static void AddressingUp(
      Dictionary<X509Name, X509Certificate> subject2CertMap,
      List<X509Certificate> orderedCertChain,
      X509Certificate current)
    {
      orderedCertChain.Insert(0, current);
      if (AntCertificationUtil.IsSelfSigned(current) || !subject2CertMap.ContainsKey(current.IssuerDN))
        return;
      X509Certificate subject2Cert = subject2CertMap[current.IssuerDN];
      AntCertificationUtil.AddressingUp(subject2CertMap, orderedCertChain, subject2Cert);
    }

    private static void AddressingDown(
      Dictionary<X509Name, X509Certificate> issuer2CertMap,
      List<X509Certificate> certChain,
      X509Certificate current)
    {
      if (!issuer2CertMap.ContainsKey(current.SubjectDN))
        return;
      X509Certificate issuer2Cert = issuer2CertMap[current.SubjectDN];
      if (AntCertificationUtil.IsSelfSigned(issuer2Cert))
        return;
      certChain.Add(issuer2Cert);
      AntCertificationUtil.AddressingDown(issuer2CertMap, certChain, issuer2Cert);
    }

    private static bool VerifyCert(X509Certificate cert, List<X509Certificate> rootCerts)
    {
      if (!cert.IsValidNow)
        return false;
      Dictionary<X509Name, X509Certificate> dictionary = new Dictionary<X509Name, X509Certificate>();
      foreach (X509Certificate rootCert in rootCerts)
        dictionary[rootCert.SubjectDN] = rootCert;
      X509Name issuerDn = cert.IssuerDN;
      if (!dictionary.ContainsKey(issuerDn))
        return false;
      X509Certificate x509Certificate = dictionary[issuerDn];
      try
      {
        AsymmetricKeyParameter publicKey = x509Certificate.GetPublicKey();
        cert.Verify(publicKey);
      }
      catch (Exception ex)
      {
        Console.WriteLine("证书验证出现异常。" + ex.Message);
        return false;
      }
      return true;
    }

    private static bool VerifyCertChain(
      List<X509Certificate> certs,
      List<X509Certificate> rootCerts)
    {
      if (!AntCertificationUtil.SortCertChain(certs))
        return false;
      X509Certificate cert1 = certs[0];
      bool flag = AntCertificationUtil.VerifyCert(cert1, rootCerts);
      if (!flag || certs.Count == 1)
        return flag;
      for (int index = 1; index < certs.Count; ++index)
      {
        try
        {
          X509Certificate cert2 = certs[index];
          if (!cert2.IsValidNow)
            return false;
          cert2.Verify(cert1.GetPublicKey());
          cert1 = cert2;
        }
        catch (Exception ex)
        {
          Console.WriteLine("证书链验证失败。" + ex.Message);
          return false;
        }
      }
      return true;
    }

    private static string CalculateMd5(string input)
    {
      using (MD5 md5_1 = (MD5) new MD5CryptoServiceProvider())
      {
        string md5_2 = "";
        foreach (byte num in md5_1.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(input)))
          md5_2 += num.ToString("x2");
        return md5_2;
      }
    }

    public static string ExtractPemPublicKeyFromCert(X509Certificate input) => Convert.ToBase64String(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(input.GetPublicKey()).GetDerEncoded());
  }