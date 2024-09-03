using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	[Serializable]
	public struct AesParameters
	{
		public string Key { get; set; }
		public string Iv { get; set; }

		public byte[] GetKeyBytes() => Convert.FromBase64String(Key);
		public byte[] GetIvBytes() => Convert.FromBase64String(Iv);

		public AesParameters(Aes aes)
		{
			Key = Convert.ToBase64String(aes.Key);
			Iv = Convert.ToBase64String(aes.IV);
		}

		public static AesParameters Create()
		{
			var aes = Aes.Create();
			aes.GenerateKey();
			aes.GenerateIV();

			return new AesParameters(aes);
		}

		public Aes CreateAes()
		{
			var aes = Aes.Create();
			aes.Key = GetKeyBytes();
			aes.IV = GetIvBytes();

			return aes;
		}

		public static Aes CreateAes(string key)
		{
			var aes = Aes.Create();
			aes.Key = Convert.FromBase64String(key);
			aes.IV = new byte[aes.BlockSize / 8];

			return aes;
		}
	}

	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#fc6b4f87fe634a828aebda3473b7b408
	/// </summary>
	public static class AesEncryptionExt
	{
		public static byte[] EncryptData(this AesParameters parameters, byte[] data)
		{
			using var aes = parameters.CreateAes();
			return EncryptData(aes, data);
		}

		public static byte[] EncryptData(this Aes aes, byte[] data)
		{
			var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			var encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

			return encryptedBytes;
		}

		public static string EncryptStringToString(this AesParameters aesParameters, string data)
		{
			return aesParameters.CreateAes().EncryptStringToString(data);
		}

		public static string EncryptStringToString(this Aes aes, string data)
		{
			var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

			using var msEncrypt = new MemoryStream();
			using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
			using var swEncrypt = new StreamWriter(csEncrypt);

			swEncrypt.Write(data);
			swEncrypt.Flush();
			csEncrypt.FlushFinalBlock();

			var encryptedBytes = msEncrypt.GetBuffer();
			return Convert.ToBase64String(encryptedBytes, 0, (int)msEncrypt.Length);
		}

		public static string AesDecryptString(this Stream encryptedBytes, AesParameters parameters)
		{
			return AesDecryptString(encryptedBytes, parameters.GetKeyBytes(), parameters.GetIvBytes());
		}

		public static string AesDecryptString(this Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var sr = new StreamReader(csDecrypt);

			return sr.ReadToEnd();
		}

		public static string DecryptString(this Aes aes, string encryptedString)
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream);

			writer.Write(encryptedString);
			writer.Flush();

			using var csDecrypt = GetCryptoStream(aes, stream);
			using var sr = new StreamReader(csDecrypt);

			return sr.ReadToEnd();
		}

		public static string AesDecryptString(this byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var sr = new StreamReader(csDecrypt);

			return sr.ReadToEnd();
		}

		public static byte[] AesDecryptData(this Stream encryptedBytes, AesParameters parameters)
		{
			return AesDecryptData(encryptedBytes, parameters.GetKeyBytes(), parameters.GetIvBytes());
		}

		public static byte[] AesDecryptData(this Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var ms = new MemoryStream();

			csDecrypt.CopyTo(ms);
			return ms.ToArray();
		}

		public static byte[] AesDecryptData(this byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var ms = new MemoryStream();

			csDecrypt.CopyTo(ms);
			return ms.ToArray();
		}

		private static CryptoStream GetCryptoStream(Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;

			return GetCryptoStream(aes, encryptedBytes);
		}

		private static CryptoStream GetCryptoStream(byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;
			return GetCryptoStream(aes, encryptedBytes);
		}

		private static CryptoStream GetCryptoStream(Aes aes, Stream encryptedBytes)
		{
			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var msDecrypt = new MemoryStream();
			encryptedBytes.CopyTo(msDecrypt);
			return new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		}

		private static CryptoStream GetCryptoStream(Aes aes, byte[] encryptedBytes)
		{
			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var msDecrypt = new MemoryStream(encryptedBytes);
			return new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		}

		public static void AesDecryptLines(this SimpleList<string> encryptedStrings, AesParameters parameters)
		{
			var aes = parameters.CreateAes();

			for (var i = 0; i < encryptedStrings.Count; i++)
			{
				var data = Convert.FromBase64String(encryptedStrings[i]);

				using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
				using var msDecrypt = new MemoryStream(data);
				using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
				using var srDecrypt = new StreamReader(csDecrypt);

				encryptedStrings[i] = srDecrypt.ReadToEnd();
			}
		}
	}
}