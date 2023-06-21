using System.Security.Cryptography;

namespace Sapientia.Extensions
{
	public static class RsaEncryptionExtensions
	{
		public static string GetPublicKey(this RSA rsa)
		{
			var rsaParameters = rsa.ExportParameters(includePrivateParameters: false);
			return rsaParameters.ToJson();
		}

		public static byte[] RsaEncryptData(this byte[] data, string publicKey)
		{
			var rsaParameters = publicKey.FromJson<RSAParameters>();
			return RsaEncryptData(data, rsaParameters);
		}

		public static byte[] RsaEncryptData(this byte[] data, RSAParameters rsaParameters)
		{
			using var rsa = RSA.Create();
			rsa.ImportParameters(rsaParameters);
			return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
		}

		public static byte[] EncryptData(this RSA rsa, byte[] data)
		{
			return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
		}

		public static byte[] DecryptData(this RSA rsa, byte[] encryptedData)
		{
			return rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
		}
	}
}