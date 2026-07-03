---
name: foundationkit
description: How to configure and use the FoundationKit library (this repo) in a consuming ASP.NET Core project — DI registration (AddFoundationKit*), repositories, controllers, DbContext, encryptors, RabbitMQ events, pagination. Use whenever the user says "configura la librería", "usa FoundationKit", or asks to wire up repos/controllers/events/encryptors with it.
---

# FoundationKit

.NET library suite (NuGet: `FoundationKit`, `FoundationKit.Repository`, `FoundationKit.Domain`, `FoundationKit.Events`, `Foundationkit.Extensions`). Source of truth: this repo + wiki at https://github.com/Orbis25/FoundationKit/wiki.

Pick the registration combo by what the consuming project needs, then wire the matching repository/controller pair.

## 1. DI registration (`Program.cs` / `builder.Services`)

Always exactly one of these base calls, picked by whether you use Identity and/or AutoMapper:

| Need | Call |
|---|---|
| Plain BaseRepository, no Identity, no AutoMapper | nothing — works out of the box |
| BaseRepository + Identity, no mapper | `builder.Services.AddFoundationKitIdentity<TUser, TIdentityDbContext>();` |
| MapRepository + Identity | `builder.Services.AddFoundationKitIdentityWithMapper<TUser, TIdentityDbContext>(Assembly.GetExecutingAssembly());` |
| MapRepository, no Identity (AutoMapper only) | `builder.Services.AddFoundationKit(Assembly.GetExecutingAssembly());` |

`TUser : IdentityUser`, `TIdentityDbContext : IdentityDbContext<TUser>` (must subclass `FoundationKitIdentityDbContext<TUser>`, see §4).

Optional add-ons, stack freely with the above:

```csharp
// AES/RSA encryption (FoundationKit/Config/FoundationKitExtensions.cs:67)
builder.Services.AddFoundationKitEncryptor(new EncryptorOption
{
    PrivateKey = "...", PublicKey = "...", HeaderAes = "...", Enconding = Encoding.UTF8
}, ServiceLifetime.Singleton); // default lifetime; Transient/Scoped also valid
app.UseMiddleware<FoundationKitAesEncryptorMiddleware>(); // register early in pipeline

// RabbitMQ events (FoundationKit.Events/Extensions/EventExtensions.cs:15)
builder.Services.AddEvents(new RabbitConfig
{
    Host = "localhost", User = "guest", Password = "guest", // or Url = "amqp://..."
    DefaultExchange = "my-exchange",   // required
    QueuePrefix = "my-service",        // required
    DefaultExchangeType = ExchangeType.Topic, // default
    RedeliverUnackedMessages = true
});
builder.Services.AddSubscriber<MyMessage>(); // one call per message type you consume
```

`AddEvents` connects to RabbitMQ synchronously at startup, declares the exchange, scans all loaded assemblies for `IMessageHandler<T>` implementations, and registers `RabbitMqConsumerService` as a hosted background service — so handlers just need to exist, no manual registration.

## 2. Repository pattern — pick ONE per entity

**BaseRepository** (async, no mapping) — `FoundationKit.Repository/Services/BaseRepository.cs`

```csharp
public interface IPersonService : IBaseRepository<Person> { }
public class PersonService(AppDbContext ctx) : BaseRepository<AppDbContext, Person>(ctx), IPersonService { }
```
`Person : BaseModel` (or `BaseBasicModel`). Methods: `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `SoftRemoveAsync` (sets `IsDeleted`), `GetAll`/`GetListAsync`/`GetPaginatedListAsync`, `ExistAsync`, `CountAsync`, `CommitAsync`.

**MapRepository** (sync, AutoMapper-backed, separate Input/Edit/Output DTOs) — `FoundationKit.Repository/Services/MapRepository.cs`

```csharp
public interface IPersonService : IMapRepository<PersonInput, PersonEdit, PersonDto> { }
public class PersonService(AppDbContext ctx, IMapper mapper)
    : MapRepository<AppDbContext, Person, PersonInput, PersonEdit, PersonDto>(ctx, mapper), IPersonService { }
```
Requires `AddFoundationKit`/`...WithMapper` (registers AutoMapper) and AutoMapper profiles mapping `Person <-> PersonInput/PersonEdit/PersonDto` in the scanned assembly. Method names drop the `Async` suffix (`Create`, `Update`, `GetAll`, `GetList`, `GetPaginatedList`).

## 3. Controllers — match the controller base to the repository pattern

`FoundationKit/Core/Controllers/`

- `ApiCoreController<TModel, TService>` where `TService : IBaseRepository<TModel>` → use with **BaseRepository**. Gives `GetByIdAsync`, `AddAsync`, `DeleteAsync` (soft delete), `GetAllPaginatedAsync`, `UpdateAsync`.
- `ApiMapController<...>` → use with **MapRepository**, same shape but sync names and DTOs.
- `MvcCoreController<...>` → Razor views/MVC, GET+POST `Create`/`Update`, `Index`, `Delete`, `GetUserId()`, `ShowAlert(...)` (SweetAlert2, override `CreateSuccess`/`UpdateSuccess` messages).

```csharp
[Route("api/people")]
public class PeopleController(IPersonService service) : ApiCoreController<Person, IPersonService>(service) { }
```

## 4. DbContext

Subclass instead of bare `DbContext`/`IdentityDbContext` to get automatic `CreatedAt`/`UpdatedAt` stamping on `BaseModel` entities (in overridden `SaveChanges`/`SaveChangesAsync`):

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : FoundationKitDbContext(options) { }
// or, with Identity:
public class AppIdentityDbContext(DbContextOptions options) : FoundationKitIdentityDbContext<User>(options) { }
```

For soft-delete query filters per entity, extend `EFCoreConfiguration<TEntity>` and override `ConfigureEF`; configs are picked up via assembly scan on model creation. For multi-schema setups implement `IDbContextSchema { string Schema { get; } }` and use `MigrationDbContextConfigAssembly` (manually patch generated migrations to use `_schema.Schema`).

## 5. Base model/DTO hierarchy

`BaseModel` (Id: Guid, CreatedAt, UpdatedAt, IsDeleted, CreatedBy, UpdatedBy) → `BaseBasicModel` (same but Created/UpdatedBy are `[NotMapped]`). `BaseInput` = empty marker for create DTOs. `BaseEdit` = edit DTO, audit fields `[JsonIgnore]`. `BaseOutput` = response DTO (Id, CreatedAt, CreatedAtStr, CreatedBy).

## 6. Pagination

`Paginate` (query-bound: `Page=1`, `Qyt=10`, `OrderByDesc`, `NoPaginate`) is the input to every `GetPaginatedList(Async)` call; response is `PaginationResult<T>` (ActualPage, Qyt, PageTotal, Total, Results).

## 7. RabbitMQ events (`FoundationKit.Events`)

```csharp
public class OrderCreated : IMessage { public Guid OrderId { get; set; } }

public class OrderCreatedHandler : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated data, CancellationToken ct) { ... }
}
```
Publish: inject `IRabbitMessageBroker`, call `await broker.PublishAsync(new OrderCreated{...})` — wraps in `EventMessage<T>` with metadata, sets RabbitMQ message `Type` for routing.
Consume: implement `IMessageHandler<T>` anywhere in a loaded assembly (auto-discovered, no manual DI registration) + call `AddSubscriber<T>()` once at startup to declare/bind its queue. `RabbitMqConsumerService` (hosted service) acks on success, nacks (optionally requeues per `RedeliverUnackedMessages`) on handler exception.

## 8. Encryption (`Foundationkit.Extensions` / `FoundationKit`)

⚠️ The wiki names `Encrypt`/`Decrypt`, but `IEncryptorService` (`FoundationKit/infrastructure/Interfaces/IEncryptorService.cs`) actually exposes — trust this over the wiki:

```csharp
T?     DecryptCore<T>(string? cipherText, string? pbKey = null, string? pvKey = null); // TweetNaCl box, falls back to EncryptorOption.PublicKey/PrivateKey
string EncryptCore<T>(T? obj, string? pbKey = null, string? pvKey = null);             // TweetNaCl box
string AESEncrypt<T>(T? obj, AesConfig config);       // AesConfig { Key, Iv } both Base64
T?     AESDecrypt<T>(string data, AesConfig config);
string? AESDecrypt(string data, AesConfig config);    // raw-string overload
string? HeaderAes { get; }                            // from EncryptorOption.HeaderAes
```

`DecryptCore`/`EncryptCore` use TweetNaCl box encryption (`CipherTweetNaCl`, `Foundationkit.Extensions/Encryptions/TNCI/`) keyed by the RSA-style Base64 `PublicKey`/`PrivateKey` pair from `EncryptorOption` — NOT RSA in the .NET sense, despite the wiki's "RSA key generation" framing. `AESEncrypt`/`AESDecrypt` are separate, plain AES, keyed by a per-call `AesConfig`.

`FoundationKitAesEncryptorMiddleware` (`app.UseMiddleware<...>()`, register early) reads the `HeaderAes` request header, expects it to be a `DecryptCore<AesConfig>`-decryptable token carrying the AES key/IV for that request, then `AESDecrypt`s the JSON body (`{"Data": "..."}` shape, see `RequestAesBase`) and replaces `context.Request.Body` before calling `_next`. Returns 401 if the header is missing/undecryptable, 400 if the body can't be decrypted. Use this when the client negotiates a per-request AES key out-of-band (encrypted with the server's TweetNaCl public key) — it's a protocol, not generic body encryption middleware.

## 9. Minimal API endpoint handlers (`FoundationKit.API`)

Alternative to controllers for minimal-API style projects:

```csharp
public class PeoplesHandler : IEndpointRouteHandler
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/people", GetAll).WithTags("People");
    }
    private IResult GetAll() => TypedResults.Ok("people here");
}
```
Then in `Program.cs`, after building the app: `app.UseFoundationKitApiHandlers(Assembly.GetExecutingAssembly());` — scans the assembly for parameterless-constructible `IEndpointRouteHandler` implementations and calls `MapEndpoints` on each. No manual registration per handler needed, same auto-discovery pattern as RabbitMQ handlers.

## 10. Misc extensions / options

- `enumValue.GetAttribute()` (`EnumExtension`) — reads `[Display(Name=...)]`, falls back to the member name.
- `controller.SendNotification(message, NotificationType)` + `@Html.RenderAlerts(TempData)` in the view — SweetAlert2 notifications via `TempData["Notification"]`. `NotificationType`/`MvcCoreNotification`: Success, Error, Question, Warning, Info. Requires SweetAlert2 JS/CSS in the page.
- `FoundationKitStaticOptions.DateUtc` (static bool, default false) — flip to `true` if `FoundationKitDbContext`/`FoundationKitIdentityDbContext` should stamp `CreatedAt`/`UpdatedAt` in UTC instead of local time.

## Gaps / verify before relying on

- TweetNaCl primitives below `CipherTweetNaCl` (`Asymmetric/Sign`, low-level `Core`) are not exercised by any documented public API path — only `DecryptCore`/`EncryptCore` (box encryption) are wired up. Read source directly if a task needs signing.
- No automated tests cover `AddFoundationKit*`, `RabbitMqConsumerService`, `PublishException`, or the AES middleware per codegraph — be extra careful editing those, verify manually.
