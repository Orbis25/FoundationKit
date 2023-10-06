namespace FoundationKit.Helpers.Encryptor;

public static class EncryptorHelper
{
    public static IActionResult AesOk<T>(this ControllerBase controller,T? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var encriptor = controller.HttpContext.RequestServices.GetRequiredService<IEncryptorService>();

        if (encriptor == null)
            throw new ArgumentNullException(nameof(IEncryptorService));

        var aesConfigHeader = controller.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == encriptor.HeaderAes).Value.ToString();

        if (string.IsNullOrEmpty(aesConfigHeader))
        {
            return new UnauthorizedResult();
        }

        var aesConfig = encriptor.DecryptCore<AesConfig>(aesConfigHeader);
        if (aesConfig == null)
        {
            return new UnauthorizedResult();
        }

        var cipherText = encriptor.AESEncrypt(obj, aesConfig);

        if(string.IsNullOrEmpty(cipherText))
            return new BadRequestResult();

        var response = new RequestAesBase()
        {
            Data = cipherText
        };

        return new OkObjectResult(response);
    }
}
