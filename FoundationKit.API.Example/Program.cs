using Foundationkit.Middlewares;
using FoundationKit.Extensions;
using System.Reflection;
using FoundationKit.API.Extensions;
using FoundationKit.Domain.Option;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FoundationKitStaticOptions.DateUtc = true;
//foundation kit config
builder.Services.AddFoundationKitIdentityWithMapper<User, ApplicationIdentityDbContext>(Assembly.GetExecutingAssembly());

builder.Services.AddFoundationKitEncryptor(new()
{
    PublicKey = "",
    PrivateKey = "",
    HeaderAes = "AES"
});

//database with postgres
builder.Services.AddDbContext<ApplicationIdentityDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));

//database without identity
//builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));

//service
//builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IPersonMapService, PersonMapService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseFoundationKitApiHandlers(Assembly.GetExecutingAssembly());

//for middleware encrypt
//app.UseMiddleware<FoundationKitAesEncryptorMiddleware>();




//app.MapPost("/api/person", async ([FromServices] IPersonService service,
//    Person person,
//    CancellationToken cancellationToken) =>
//{
//    return await service.CreateAsync(person, cancellationToken);
//})
//.WithName("add");

//app.MapPut("/api/person", async ([FromServices] IPersonService service,
//    Person person,
//    CancellationToken cancellationToken) =>
//{
//    return await service.UpdateAsync(person, cancellationToken);
//})
//.WithName("update");

//app.MapGet("/api/person", async (
//    [FromServices] IPersonService service,
//    CancellationToken cancellationToken) =>
//{
//    return await service.GetListAsync(cancellationToken: cancellationToken);
//})
//.WithName("get all");


//app.MapGet("/api/person/{id}", async (
//    [FromServices] IPersonService service,
//    Guid id,
//    CancellationToken cancellationToken
//    ) =>
//{
//    return await service.GetByIdAsync(id, cancellationToken: cancellationToken);
//})
//.WithName("get one");


app.MapControllers();

app.Run();