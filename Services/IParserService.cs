using TestProd.Models;

namespace TestProd.Services
{
    public interface IParserService
    {
        Task<ResponseModel> ProcessAsync(InputModel input);
    }
}
