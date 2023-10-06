namespace Foundationkit.Extensions.Encryptions.AES;

/// <summary>
/// Encryption helper class
/// </summary>
public static class AesEncrytionHelper
{
    /// <summary>
    /// Intizialization vector for ECB
    /// </summary>
    private static readonly byte[] ivZero = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    /// <summary>
    /// Encrypts a plain text with AES
    /// </summary>
    /// <param name="plainText">Text to encript</param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES initial vector</param>
    /// <returns>Encrypted text</returns>
    public static byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
    {
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Create an encryptor to perform the stream transform.
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }

            encrypted = msEncrypt.ToArray();
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    /// <summary>
    /// Decrypts a ciphered text
    /// </summary>
    /// <param name="cipherText">Ciphered text</param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES initial vector</param>
    /// <returns>Decrytpted text</returns>
    public static string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
    {
        // Declare the string used to hold
        // the decrypted text.
        string? plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using MemoryStream msDecrypt = new(cipherText);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            plaintext = srDecrypt.ReadToEnd();

        }

        return plaintext;
    }

    /// <summary>
    /// Converts a hex string to a byte array
    /// </summary>
    /// <param name="hex">Hex string</param>
    /// <returns>Byte array</returns>
    public static byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    public static byte[] EncryptECB(string plainText, byte[] key)
    {
        #pragma warning disable SYSLIB0021 // Type or member is obsolete
        using AesManaged aesAlg = new() 
        {
            KeySize = 128,
            Key = key,
            BlockSize = 128,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7,
            IV = ivZero
        };
        #pragma warning restore SYSLIB0021 // Type or member is obsolete
        using ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        byte[] encrypted;

        // Create the streams used for encryption.
        using (var msEncrypt = new MemoryStream())
        {
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt, Encoding.ASCII))
            {
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }

            encrypted = msEncrypt.ToArray();

        }

        return encrypted;
    }

    public static string DencryptECB(byte[] cipherText, byte[] key)
    {
        #pragma warning disable SYSLIB0021 // Type or member is obsolete
        using AesManaged aesAlg = new() 
        {
            KeySize = 128,
            Key = key,
            BlockSize = 128,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7,
            IV = ivZero
        };
        #pragma warning restore SYSLIB0021 // Type or member is obsolete
        using ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        // Declare the string used to hold
        // the decrypted text.
        string? plaintext = null;

        // Create the streams used for decryption.
        using (MemoryStream msDecrypt = new(cipherText))
        {
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt, Encoding.UTF8);
            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            plaintext = srDecrypt.ReadToEnd();
        }

        return plaintext;
    }
}
