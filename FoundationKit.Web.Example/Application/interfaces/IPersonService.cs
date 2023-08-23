using FoundationKit.Repository.Interfaces;
using FoundationKit.Web.Example.Domain.Models;

namespace FoundationKit.Web.Example.Application.interfaces;
public interface IPersonService : IBaseRepository<Person>
{
}