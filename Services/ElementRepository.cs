using Dapper;
using Npgsql;
using TestProd.Models;

namespace TestProd.Services
{
    public class ElementRepository : IElementRepository
    {
        private readonly string _connectionString;

        public ElementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения не настроена");
        }

        public async Task AddRangeAsync(IEnumerable<Element> elements)
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                const string sql = """
                    INSERT INTO elements (attribute_value, element_html)
                    VALUES (@AttributeValue, @ElementHtml);
                    """;

                await connection.ExecuteAsync(sql, elements);
            }
        }
    }
}
