AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//database with postgres
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));

//service
builder.Services.AddScoped<IPersonService, PersonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/api/person", async ([FromServices] IPersonService service,
    Person person,
    CancellationToken cancellationToken) =>
{
    return await service.Create(person, cancellationToken);
})
.WithName("add");

app.MapPut("/api/person", async ([FromServices] IPersonService service,
    Person person,
    CancellationToken cancellationToken) =>
{
    return await service.Update(person, cancellationToken);
})
.WithName("update");

app.MapGet("/api/person", async (
    [FromServices] IPersonService service,
    CancellationToken cancellationToken) =>
{
    return await service.GetList(cancellationToken: cancellationToken);
})
.WithName("get all");


app.MapGet("/api/person/{id}", async (
    [FromServices] IPersonService service,
    Guid id,
    CancellationToken cancellationToken
    ) =>
{
    return await service.GetById(id, cancellationToken: cancellationToken);
})
.WithName("get one");

app.Run();