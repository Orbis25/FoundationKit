using Foundationkit.Extensions.Encryptions.AES;
using FoundationKit.API.Example.Domain.Mappings;
using FoundationKit.Core.Controllers;
using FoundationKit.Helpers.Encryptor;
using FoundationKit.Infrastructure.Interfaces;
using System.Security.Cryptography;

namespace FoundationKit.API.Example.Controllers;

public class PeopleController : ApiMapController<IPersonMapService, PersonDto, PersonInput, PersonEdit>
{
    private readonly IPersonMapService _service;
    private readonly IEncryptorService _encryptor;
    public PeopleController(IPersonMapService service, IEncryptorService encryptor) : base(service)
    {
        _service = service;
        _encryptor = encryptor;
    }



    [HttpPost("/test")]
    public async Task<IActionResult> Test([FromBody] AesConfig config)
    {
        return Ok(_encryptor.AESEncrypt(config, config));
    }


    [HttpPost("/decrypt")]
    public async Task<IActionResult> Decrypt([FromBody] AesConfig config)
    {
        _ = config;
        return this.AesOk(config);
    }


    [HttpGet("/decrypt-system")]
    public async Task<IActionResult> Decryptstr(string key, string iv)
    {
        using var aes = Aes.Create();
        var r = new AesConfig()
        {
            Iv = Convert.ToBase64String(aes.IV),
            Key = Convert.ToBase64String(aes.Key),
        };
        return Ok(_encryptor.EncryptCore(r));
    }
}
