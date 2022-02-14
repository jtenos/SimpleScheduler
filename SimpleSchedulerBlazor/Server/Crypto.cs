using System.Security.Cryptography;

namespace SimpleSchedulerBlazor.Server;

internal static class Crypto
{
    private const int IV_SIZE_BYTES = 16;

    public static byte[] Encrypt(byte[] input, byte[] cryptoKey, byte[] authKey)
    {
        byte[] iv = new byte[IV_SIZE_BYTES];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        byte[] cipherText;
        using (Aes algo = Aes.Create())
        {
            algo.Mode = CipherMode.CBC;
            using ICryptoTransform encryptor = algo.CreateEncryptor(cryptoKey, iv);
            using MemoryStream inputStream = new(input);
            using MemoryStream memoryStream = new();
            using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using BinaryWriter binaryWriter = new(cryptoStream);
                byte[] buffer = new byte[0x4000];
                int count;
                while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    binaryWriter.Write(buffer, 0, count);
                }
            }
            cipherText = memoryStream.ToArray();
        }

        using (HMACSHA256 hashAlgo = new(authKey))
        {
            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                binaryWriter.Write(iv);
                binaryWriter.Write(cipherText);
                binaryWriter.Flush();
                byte[] tag = hashAlgo.ComputeHash(memoryStream.ToArray());
                binaryWriter.Write(tag);
            }
            return memoryStream.ToArray();
        }
    }

    public static byte[] Decrypt(byte[] input, byte[] cryptoKey, byte[] authKey)
    {
        using HMACSHA256 hashAlgo = new(authKey);

        // The last 32 bytes of the file.
        byte[] sentTag = new byte[hashAlgo.HashSize / 8];
        Array.Copy(input, input.Length - sentTag.Length, sentTag, 0, sentTag.Length);

        // The calculated tag based on all but the last 32 bytes of the file.
        byte[] calcTag = hashAlgo.ComputeHash(input, 0, input.Length - sentTag.Length);

        if (!sentTag.SequenceEqual(calcTag))
        {
            throw new CryptographicException("Authorization failed.");
        }

        byte[] iv = new byte[IV_SIZE_BYTES];
        Array.Copy(input, 0, iv, 0, iv.Length);

        // Pull the IV and the sent tag out of the input, leaving only the ciphertext.
        byte[] tmp = new byte[input.Length - iv.Length - sentTag.Length];
        Array.Copy(input, iv.Length, tmp, 0, tmp.Length);
        input = tmp;

        using Aes algo = Aes.Create();
        algo.Mode = CipherMode.CBC;
        using MemoryStream inputStream = new MemoryStream(input);
        using ICryptoTransform decryptor = algo.CreateDecryptor(cryptoKey, iv);
        using MemoryStream outputStream = new();
        using (CryptoStream decryptorStream = new(outputStream, decryptor, CryptoStreamMode.Write))
        {
            using BinaryWriter binaryWriter = new(decryptorStream);
            byte[] buffer = new byte[0x4000];
            int count;
            while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                binaryWriter.Write(buffer, 0, count);
            }
        }
        return outputStream.ToArray();
    }
}
