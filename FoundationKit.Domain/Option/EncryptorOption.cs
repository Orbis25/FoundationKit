namespace FoundationKit.Domain.Option;

public class EncryptorOption
{
    public string? PrivateKey { get; set; }
    public string? PublicKey { get; set; }
    public Encoding? Enconding { get; set; }
    public string? HeaderAes { get; set; }
}
