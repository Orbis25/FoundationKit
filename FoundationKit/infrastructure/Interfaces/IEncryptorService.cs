namespace FoundationKit.Infrastructure.Interfaces;

public interface IEncryptorService
{
    T? DecryptCore<T>(string? cipherText, string? pbKey = null, string? pvKey = null);
    string EncryptCore<T>(T? obj, string? pbKey = null, string? pvKey = null);
    string AESEncrypt<T>(T? obj, AesConfig config);
    T? AESDecrypt<T>(string data, AesConfig config);
    string? AESDecrypt(string data, AesConfig config);
    string? HeaderAes { get; }
}
