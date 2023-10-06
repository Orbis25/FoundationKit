namespace FoundationKit.infrastructure.Services;

public class EncryptorService : IEncryptorService
{
    private readonly EncryptorOption _options;


    public EncryptorService(EncryptorOption options)
    {
        _options = options;
    }

    public string? HeaderAes => _options.HeaderAes;

    /// <summary>
    /// CryptoBoxAfternm TweetNaCl encription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cipherText"></param>
    /// <param name="pbkey"></param>
    /// <param name="pvKey"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T? DecryptCore<T>(string? cipherText, string? pbKey = null, string? pvKey = null)
    {
        if (string.IsNullOrEmpty(cipherText))
            return default;

        //validate is valid base64
        if ((string.IsNullOrEmpty(pbKey) && string.IsNullOrEmpty(_options.PublicKey))
            || (string.IsNullOrEmpty(pvKey) && string.IsNullOrEmpty(_options.PrivateKey)))
            throw new ArgumentNullException("pbkey/pvkey");

        var cipherData = Convert.FromBase64String(cipherText);

        var nonce = cipherData.AsEnumerable().Skip(0).Take(CipherTweetNaCl.BoxNonceBytes).ToArray();
        var message = cipherData.AsEnumerable().Skip(CipherTweetNaCl.BoxNonceBytes).Take(cipherData.Length).ToArray();

        var key = SharedFromPair(pbKey ?? _options.PublicKey ?? "", pvKey ?? _options.PrivateKey ?? "");

        var data = CipherTweetNaCl.CryptoBoxOpenAfternm(message, nonce, key);

        var dataStr = (_options.Enconding ?? Encoding.UTF8).GetString(data);

        return JsonConvert.DeserializeObject<T>(dataStr);

    }

    public string EncryptCore<T>(T? obj, string? pbKey = null, string? pvKey = null)
    {
        if (obj == null)
            return string.Empty;

        //validate is valid base64
        if ((string.IsNullOrEmpty(pbKey) && string.IsNullOrEmpty(_options.PublicKey))
            || (string.IsNullOrEmpty(pvKey) && string.IsNullOrEmpty(_options.PrivateKey)))
            throw new ArgumentNullException("pbkey/pvkey");

        var info = JsonConvert.SerializeObject(obj);
        var data = (_options.Enconding ?? Encoding.UTF8).GetBytes(info);

        var nonce = GetRamdomNonce(CipherTweetNaCl.BoxNonceBytes);
        var key = SharedFromPair(pbKey ?? _options.PublicKey ?? "", pvKey ?? _options.PrivateKey ?? "");

        var cipherText = CipherTweetNaCl.CryptoBoxAfternm(data, nonce, key);

        var fullMessage = new List<byte>(nonce.Length + cipherText.Length);
        fullMessage.InsertRange(0, nonce);
        fullMessage.InsertRange(nonce.Length, cipherText);

        return Convert.ToBase64String(fullMessage.ToArray());
    }

    public string AESEncrypt<T>(T? obj, AesConfig config)
    {
        if(obj == null) return string.Empty;

        if (string.IsNullOrEmpty(config.Iv) || string.IsNullOrEmpty(config.Key))
            throw new ArgumentNullException(nameof(config));

        try
        {
            var text = JsonConvert.SerializeObject(obj);

            var cipherText = AesEncrytionHelper.EncryptStringToBytesAes(text,
                Convert.FromBase64String(config.Key),
                Convert.FromBase64String(config.Iv));

            return Convert.ToBase64String(cipherText);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex?.Message);
            return string.Empty;
        }
    }

    public T? AESDecrypt<T>(string data, AesConfig config)
    {
        var dataAsBytes =  Convert.FromBase64String(data);

        if (string.IsNullOrEmpty(config.Iv) || string.IsNullOrEmpty(config.Key))
            throw new ArgumentNullException(nameof(config));

        try
        {
            var json = AesEncrytionHelper.DecryptStringFromBytesAes(dataAsBytes,
                Convert.FromBase64String(config.Key),
                Convert.FromBase64String(config.Iv));

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex?.Message);
            return default;
        }

    }

    public string? AESDecrypt(string data, AesConfig config)
    {
        var dataAsBytes = Convert.FromBase64String(data);

        if (string.IsNullOrEmpty(config.Iv) || string.IsNullOrEmpty(config.Key))
            throw new ArgumentNullException(nameof(config));

        try
        {
            var json = AesEncrytionHelper.DecryptStringFromBytesAes(dataAsBytes,
                Convert.FromBase64String(config.Key),
                Convert.FromBase64String(config.Iv));

            if (string.IsNullOrEmpty(json))
                return default;

            return json;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex?.Message);
            return default;
        }

    }

    /// <summary>
    /// Return a precomputed shared key
    /// </summary>
    /// <param name="pbkey">public key</param>
    /// <param name="pvKey">private key</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static byte[] SharedFromPair(string pbkey, string pvKey)
    {
        if (string.IsNullOrEmpty(pvKey) || string.IsNullOrEmpty(pbkey))
            throw new ArgumentNullException(nameof(pvKey));

        var publicKey = Convert.FromBase64String(pbkey);
        var privatKey = Convert.FromBase64String(pvKey);

        return CipherTweetNaCl.CryptoBoxBeforenm(publicKey, privatKey);
    }

    private static byte[] GetRamdomNonce(int lenght)
    {
        var nonce = new byte[lenght];
        RandomNumberGenerator.Create().GetBytes(nonce);
        return nonce;
    }
}
