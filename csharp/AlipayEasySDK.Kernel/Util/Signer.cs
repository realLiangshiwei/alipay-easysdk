using System.Security.Cryptography;
using System.Text;

namespace Alipay.EasySDK.Kernel.Util;

 public class Signer
  {
    public static string Sign(string content, string privateKeyPem)
    {
      try
      {
        using (RSACryptoServiceProvider cryptoServiceProvider = Signer.BuildRSAServiceProvider(Convert.FromBase64String(privateKeyPem)))
        {
          byte[] bytes = AlipayConstants.DEFAULT_CHARSET.GetBytes(content);
          return Convert.ToBase64String(cryptoServiceProvider.SignData(bytes, (object) "SHA256"));
        }
      }
      catch (Exception ex)
      {
        string message = "签名遭遇异常，content=" + content + " privateKeySize=" + privateKeyPem.Length.ToString() + " reason=" + ex.Message;
        Console.WriteLine(message);
        throw new Exception(message, ex);
      }
    }

    public static bool Verify(string content, string sign, string publicKeyPem)
    {
      try
      {
        using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
        {
          cryptoServiceProvider.PersistKeyInCsp = false;
          cryptoServiceProvider.ImportParameters(Signer.ConvertFromPemPublicKey(publicKeyPem));
          return cryptoServiceProvider.VerifyData(AlipayConstants.DEFAULT_CHARSET.GetBytes(content), (object) "SHA256", Convert.FromBase64String(sign));
        }
      }
      catch (Exception ex)
      {
        string message = "验签遭遇异常，content=" + content + " sign=" + sign + " publicKey=" + publicKeyPem + " reason=" + ex.Message;
        Console.WriteLine(message);
        throw new Exception(message, ex);
      }
    }

    public static bool VerifyParams(Dictionary<string, string> parameters, string publicKeyPem)
    {
      string parameter = parameters["sign"];
      parameters.Remove("sign");
      parameters.Remove("sign_type");
      return Signer.Verify(Signer.GetSignContent((IDictionary<string, string>) parameters), parameter, publicKeyPem);
    }

    private static string GetSignContent(IDictionary<string, string> parameters)
    {
      IEnumerator<KeyValuePair<string, string>> enumerator = ((IEnumerable<KeyValuePair<string, string>>) new SortedDictionary<string, string>(parameters, (IComparer<string>) StringComparer.Ordinal)).GetEnumerator();
      StringBuilder stringBuilder = new StringBuilder("");
      while (enumerator.MoveNext())
      {
        string key = enumerator.Current.Key;
        string str = enumerator.Current.Value;
        stringBuilder.Append(key).Append("=").Append(str).Append("&");
      }
      return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
    }

    private static RSAParameters ConvertFromPemPublicKey(string pemPublickKey)
    {
      pemPublickKey = !string.IsNullOrEmpty(pemPublickKey) ? pemPublickKey.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "") : throw new Exception("PEM格式公钥不可为空。");
      byte[] sourceArray = Convert.FromBase64String(pemPublickKey);
      bool flag1 = sourceArray.Length == 162;
      bool flag2 = sourceArray.Length == 294;
      if (!(flag1 | flag2))
        throw new Exception("公钥长度只支持1024和2048。");
      byte[] destinationArray1 = flag1 ? new byte[128] : new byte[256];
      byte[] destinationArray2 = new byte[3];
      Array.Copy((Array) sourceArray, flag1 ? 29 : 33, (Array) destinationArray1, 0, flag1 ? 128 : 256);
      Array.Copy((Array) sourceArray, flag1 ? 159 : 291, (Array) destinationArray2, 0, 3);
      return new RSAParameters()
      {
        Modulus = destinationArray1,
        Exponent = destinationArray2
      };
    }

    private static RSACryptoServiceProvider BuildRSAServiceProvider(byte[] privateKey)
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) new MemoryStream(privateKey)))
      {
        switch (binaryReader.ReadUInt16())
        {
          case 33072:
            int num1 = (int) binaryReader.ReadByte();
            break;
          case 33328:
            int num2 = (int) binaryReader.ReadInt16();
            break;
          default:
            return (RSACryptoServiceProvider) null;
        }
        if (binaryReader.ReadUInt16() != (ushort) 258 || binaryReader.ReadByte() != (byte) 0)
          return (RSACryptoServiceProvider) null;
        int integerSize1 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray1 = binaryReader.ReadBytes(integerSize1);
        int integerSize2 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray2 = binaryReader.ReadBytes(integerSize2);
        int integerSize3 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray3 = binaryReader.ReadBytes(integerSize3);
        int integerSize4 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray4 = binaryReader.ReadBytes(integerSize4);
        int integerSize5 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray5 = binaryReader.ReadBytes(integerSize5);
        int integerSize6 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray6 = binaryReader.ReadBytes(integerSize6);
        int integerSize7 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray7 = binaryReader.ReadBytes(integerSize7);
        int integerSize8 = Signer.GetIntegerSize(binaryReader);
        byte[] numArray8 = binaryReader.ReadBytes(integerSize8);
        RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
        cryptoServiceProvider.ImportParameters(new RSAParameters()
        {
          Modulus = numArray1,
          Exponent = numArray2,
          D = numArray3,
          P = numArray4,
          Q = numArray5,
          DP = numArray6,
          DQ = numArray7,
          InverseQ = numArray8
        });
        return cryptoServiceProvider;
      }
    }

    private static int GetIntegerSize(BinaryReader binaryReader)
    {
      if (binaryReader.ReadByte() != (byte) 2)
        return 0;
      byte num1 = binaryReader.ReadByte();
      int integerSize;
      switch (num1)
      {
        case 129:
          integerSize = (int) binaryReader.ReadByte();
          break;
        case 130:
          byte num2 = binaryReader.ReadByte();
          integerSize = BitConverter.ToInt32(new byte[4]
          {
            binaryReader.ReadByte(),
            num2,
            (byte) 0,
            (byte) 0
          }, 0);
          break;
        default:
          integerSize = (int) num1;
          break;
      }
      while (binaryReader.ReadByte() == (byte) 0)
        --integerSize;
      binaryReader.BaseStream.Seek(-1L, SeekOrigin.Current);
      return integerSize;
    }
  }