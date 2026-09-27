using TestProd.Models;

namespace TestProd.Services
{
    public interface IElementRepository
    {
        Task AddRangeAsync(IEnumerable<Element> elements);
    }
}
