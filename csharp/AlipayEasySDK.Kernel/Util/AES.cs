using System.Security.Cryptography;

namespace Alipay.EasySDK.Kernel.Util;

 public class AES
  {
    private static readonly byte[] AES_IV = AES.InitIV(16);

    public static string Encrypt(string plainText, string key)
    {
      try
      {
        byte[] numArray = Convert.FromBase64String(key);
        byte[] bytes = AlipayConstants.DEFAULT_CHARSET.GetBytes(plainText);
        RijndaelManaged rijndaelManaged1 = new RijndaelManaged();
        rijndaelManaged1.Key = numArray;
        rijndaelManaged1.Mode = CipherMode.CBC;
        rijndaelManaged1.Padding = PaddingMode.PKCS7;
        rijndaelManaged1.IV = AES.AES_IV;
        RijndaelManaged rijndaelManaged2 = rijndaelManaged1;
        return Convert.ToBase64String(rijndaelManaged2.CreateEncryptor(rijndaelManaged2.Key, rijndaelManaged2.IV).TransformFinalBlock(bytes, 0, bytes.Length));
      }
      catch (Exception ex)
      {
        throw new Exception("AES加密失败，plainText=" + plainText + "，keySize=" + key.Length.ToString() + "。" + ex.Message, ex);
      }
    }

    public static string Decrypt(string cipherText, string key)
    {
      try
      {
        byte[] numArray = Convert.FromBase64String(key);
        byte[] inputBuffer = Convert.FromBase64String(cipherText);
        RijndaelManaged rijndaelManaged1 = new RijndaelManaged();
        rijndaelManaged1.Key = numArray;
        rijndaelManaged1.Mode = CipherMode.CBC;
        rijndaelManaged1.Padding = PaddingMode.PKCS7;
        rijndaelManaged1.IV = AES.AES_IV;
        RijndaelManaged rijndaelManaged2 = rijndaelManaged1;
        byte[] bytes = rijndaelManaged2.CreateDecryptor(rijndaelManaged2.Key, rijndaelManaged2.IV).TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return AlipayConstants.DEFAULT_CHARSET.GetString(bytes);
      }
      catch (Exception ex)
      {
        throw new Exception("AES解密失败，ciphertext=" + cipherText + "，keySize=" + key.Length.ToString() + "。" + ex.Message, ex);
      }
    }

    private static byte[] InitIV(int blockSize)
    {
      byte[] numArray = new byte[blockSize];
      for (int index = 0; index < blockSize; ++index)
        numArray[index] = (byte) 0;
      return numArray;
    }
  }