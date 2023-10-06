namespace Foundationkit.Middlewares;

public class FoundationKitAesEncryptorMiddleware
{
    private readonly RequestDelegate _next;
    public FoundationKitAesEncryptorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var encriptor = context.RequestServices.GetRequiredService<IEncryptorService>();

        if (encriptor == null)
            throw new ArgumentNullException(nameof(IEncryptorService));

        var aesConfigHeader = context.Request.Headers.FirstOrDefault(x => x.Key == encriptor.HeaderAes).Value.ToString();

        if (string.IsNullOrEmpty(aesConfigHeader))
        {
            context.Response.StatusCode = 401;
            return;
        }

        var aesConfig = encriptor.DecryptCore<AesConfig>(aesConfigHeader);
        if (aesConfig == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        var body = await BodyRequestAsBase(context);

        if (string.IsNullOrEmpty(body?.Data))
        {
            context.Response.StatusCode = 400;
            return;
        }

        var data = encriptor.AESDecrypt(body.Data, aesConfig);

        if (data == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        await UpdateBody(context, data);

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }

    private async Task<RequestAesBase?> BodyRequestAsBase(HttpContext context)
    {
        // Capture the original request body stream
        var originalBodyStream = context.Request.Body;
        RequestAesBase requestBody = new();
        try
        {
            using (var memStream = new MemoryStream())
            {
                // Replace the request body with a MemoryStream
                context.Request.Body = memStream;

                // Copy the original request body into the MemoryStream
                await originalBodyStream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                // Read the stream into a string
                var str = await new StreamReader(memStream).ReadToEndAsync();
                if (!string.IsNullOrEmpty(str))
                {
                    requestBody = JsonConvert.DeserializeObject<RequestAesBase>(str);
                }
            }
        }
        finally
        {
            // Restore the original request body stream
            context.Request.Body = originalBodyStream;
        }

        return requestBody;
    }

    private async Task UpdateBody(HttpContext context, string json)
    {
        // Capture the original request body stream
        var originalRequestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

        try
        {
            // Replace the request body with the updated content
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex?.ToString());
            // Restore the original request body stream (if needed)
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(originalRequestBody));
        }
    }
}
