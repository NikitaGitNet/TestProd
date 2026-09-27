using Microsoft.AspNetCore.Mvc;
using TestProd.Models;
using TestProd.Services;

namespace TestProd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParserController : ControllerBase
    {
        private readonly IParserService _service;

        public ParserController(IParserService service)
        {
            _service = service;
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Parse([FromBody]InputModel? input)
        {
            if (input is null)
            {
                return BadRequest(new
                {
                    is_error = 1,
                    error_code = ErrorCode.ValidationError.ToString(),
                    error_message = "Тело запроса должно быть заполнено"
                });
            }

            var result = await _service.ProcessAsync(input);
            
            return result.ErrorCode switch
            {
                // Контракт этого endpoint не создание ресурса, а обработка данных.
                // Поэтому здесь 200, а не 201
                ErrorCode.None => Ok(result),

                // Ошибки завязанные на некорректной передачи данных
                // Возвращаем 400
                ErrorCode.ValidationError or
                ErrorCode.InvalidUrlBase64 or
                ErrorCode.InvalidPageBase64 or
                ErrorCode.InvalidEncryptedTextBase64 or
                ErrorCode.InvalidKeyBase64 or
                ErrorCode.DecryptionError => BadRequest(result),

                // Если отвалилась бд или получили неизвестную ошибку
                // Возвращаем 500
                ErrorCode.DatabaseError or
                ErrorCode.UnknownError => StatusCode(StatusCodes.Status500InternalServerError, result),

                // Дефолт тоже 500
                _ => StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }
    }
}
