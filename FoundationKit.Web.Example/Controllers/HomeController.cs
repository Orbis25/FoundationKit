using FoundationKit.Core.Controllers;
using FoundationKit.Domain.Dtos.Paginations;
using FoundationKit.Infrastructure.Interfaces;
using FoundationKit.Web.Example.Application.interfaces;
using FoundationKit.Web.Example.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoundationKit.Web.Example.Controllers;

public class HomeController : MvcCoreController<Person, IPersonService>
{
    private readonly IEncryptorService _encrytor;
    public HomeController(IPersonService service, IEncryptorService encryptorService) : base(service)
    {
        _encrytor = encryptorService;
    }

    /// <summary>
    /// Test notifications
    /// </summary>
    /// <returns></returns>
    public override Task<IActionResult> Index([FromQuery] Paginate paginate, CancellationToken cancellationToken = default)
    {
        string config = $"{{" +
            $"html:'Hello from config'," +
            $"showCancelButton:true" +
            $"}}";

        //with config
        ShowAlert("Hello", FoundationKit.Domain.Enums.MvcCoreNotification.Error, "", config);
        //without config
        ShowAlert("Hello", FoundationKit.Domain.Enums.MvcCoreNotification.Success);
        return base.Index(paginate, cancellationToken);
    }


    [HttpGet("/test")]
    public async Task<IActionResult> Test()
    {
        return Ok(_encrytor.EncryptCore("HOLA DESDE OTRO SISTEMA"));
    }
}