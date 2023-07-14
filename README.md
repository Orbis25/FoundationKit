# FoundationKit

![icon](https://github.com/Orbis25/FoundationKit/assets/38229144/5a4060d0-55f7-4145-995b-7b5b3e90521e)

The fundamental kit of your application, which offers key components such as repositories, validations, base models, and configurations.

This library is separated in 4 components or nuggets:

- `FoundationKit` - Contains all implementations from the library.
- `FoundationKit.EntityFrameworkCore` - Contains the base DbContext and base models, configurations and access configuration for database.
- `FoundationKit.Repository` - Contains the base repositories.
- `FoundationKit.Extensions` - Contains the extensions for the library.

[![Release to NuGet](https://github.com/Orbis25/FoundationKit/actions/workflows/release.yml/badge.svg)](https://github.com/Orbis25/FoundationKit/actions/workflows/release.yml)

## [Roadmap]

`Only mark items are completed`

- [Repository Pattern](#repository-pattern) ✅
- Repository Pattern with Automapper
- Filter repository
- Firebase Repository
- AMQP RABBIT Integration
- Validation
  - DPI
  - DNI
  - CreditCard
  - cvv validation
  - Date validation
  - Get credit card type
- Configuration
  - Base Model ✅
    - [BaseModel](#base-model)
  - Base Dto
  - Paginate ✅
    - [Paginate](#paginate)
  - Base Mappings
  - Base EF Configuration
  - Base Response
  - API key validation with swagger
  - Base SecurityApi key Model
  - [Core API Controllers](#core-api-controllers)✅
  - Core MVC Controllers
  - Base DbContext ✅
    - [FoundationKitDbContext](#base-dbcontext)
- Services
  - Mail Service
  - SecurityKey Service
- Helpers
  - Notification Helper (MVC)
  - Export Excel
  - RSA Encryption / Decryption,
  - Format amounts
- Extensions
  - Minimals api extensions
  - ICollection extensions
  - Enum extensions ✅
    - [GetDisplayName](#getdisplayname)
- Documentation ✅

# Documentation

this documentation represent the main features of the library separated by sections.

## Repository Pattern

Is a common pattern used in the development of applications, which allows us to abstract the data access layer, in this way we can change the data source without affecting the rest of the application.

`how to use it?`

- Create a interface and Class as service and use the inheritance of the base repository, passing a model as a generic parameter when the model is a class that inherits from the base model. `BaseModel`

```csharp

//database context inheritance
public class ApplicationDbContext : FoundationKitDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Person> Persons { get; set; }
}

//using base model
public class Person : BaseModel
{
    public string? Name { get; set; }

    [NotMapped]
    public override string? CreatedBy { get; set; }
}

// inheritance base repository
public interface IPersonService
: IBaseRepository<Person>
{
}

// inheritance base repository and implementation of base repository
public class PersonService : BaseRepository<ApplicationDbContext, Person>, IPersonService
{
    public PersonService(ApplicationDbContext context) : base(context)
    {
    }
}

//usage in a minimal api endpoint
app.MapPost("/api/person", async ([FromServices] IPersonService service,
    Person person,
    CancellationToken cancellationToken) =>
    {
        return await service.Create(person, cancellationToken);
    })
    .WithName("add");

```

## Configuration

### Base Model

is a base class named `BaseModel` that allows us to have a common structure in our models, it has the following properties:

- `Id` - is a unique identifier for the model.
- `CreatedAt` - is the date of creation of the model.
- `UpdateAt` - is the date of the last update of the model.
- `CreatedAtStr` - is the date of creation of the model in string format. (dd/MM/yyyy hh:mm:ss)
- `UpdateAtStr` - is the date of the last update of the model in string format. (dd/MM/yyyy hh:mm:ss)
- `IsDeleted` - is a flag that indicates if the model is deleted.
- `CreatedBy` - is the user who created the model.
- `UpdatedBy` - is the user who updated the model.

### Paginate

is a class named `Paginate` that allows us to paginate a list of models; please see the paginate class.

### Core API Controllers

Contains the `CoreApiController` class which inherits from `ControllerBase` and contains the next characteristics:

- `GetByIdAsync` - is a method that allows us to obtain a model by its id.
- `GetAllPaginatedAsync` - is a method that allows us to obtain all the models paginated or not please see [Paginate](#paginate).
- `AddAsync` - is a method that allows us to create a model.
- `UpdateAsync` - is a method that allows us to update a model.
- `DeleteAsync` - is a method that allows up to update a model with the `IsDeleted` flag in true.

Example in `FoundationKit.API.Example/Controllers/PersonController.cs`

### Base DbContext

Contains the `FoundationKitDbContext` class which inherits from `DbContext` and contains the next characteristics:

- `SaveChanges`: Override the `SaveChanges` method to set the `CreatedAt` and `UpdateAt` properties of the models.
- `SaveChangesAsync`: Override the `SaveChangesAsync` method to set the `CreatedAt` and `UpdateAt` properties of the models.

## Extensions

### `GetDisplayName`

This extension allows us to obtain the name of the enum value based in the `[Display]` attribute if it exists, otherwise it returns the name of the enum based in your code.
