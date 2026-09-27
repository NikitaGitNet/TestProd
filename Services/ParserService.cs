using AngleSharp;
using AngleSharp.Dom;
using FluentValidation;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TestProd.Models;
using ElementEntity = TestProd.Models.Element;

namespace TestProd.Services
{
    public class ParserService : IParserService
    {
        private static readonly Regex EmailRegex = new Regex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
        private readonly IValidator<InputModel> _validator;
        private readonly IElementRepository _elementRepository;

        public ParserService(IValidator<InputModel> validator, IElementRepository elementRepository)
        {
            _validator = validator;
            _elementRepository = elementRepository;
        }

        private static byte[] DecodeBase64(string value) => Convert.FromBase64String(value);

        /// <summary>
        /// Парсит HTML страницу в DOM объект с использованием AngleSharp.
        /// </summary>
        /// <param name="page">HTML страница</param>
        /// <returns></returns>
        private static async Task<IDocument> ParsePageToDomAsync(string page)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            return await context.OpenAsync(request => request.Content(page));
        }

        /// <summary>
        /// Получаем enities из коллекции элементов
        /// </summary>
        /// <param name="elements">Коллекция элементов полученная по целевому селектору</param>
        /// <param name="attribute">Целевой атрибут</param>
        /// <returns></returns>
        private static List<ElementEntity> ExtractElements(
            IEnumerable<IElement> elements,
            string attribute
            )
        {
            return elements
                .Select(element => new ElementEntity
                {
                    AttributeValue = element.GetAttribute(attribute),
                    ElementHtml = element.OuterHtml
                })
                .ToList();
        }

        /// <summary>
        /// Расшифровываем текст
        /// --------------------
        /// Вообще по хорошему для дешифровки надо бы отдельный сервис создать,
        /// но тк это тестовый проект, для удобства чтения оставлю это тут
        /// </summary>
        /// <param name="encryptedTextBytesB64">Текст</param>
        /// <param name="keyBytesB64">Ключ</param>
        /// <returns>Расшифрованный текст</returns>
        private static string DecryptText(
            byte[] encryptedBytes,
            byte[] keyBytes
            )
        {
            string result;

            using var aes = Aes.Create();

            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using (var decryptor = aes.CreateDecryptor())
            {
                var decryptedBytes = decryptor.TransformFinalBlock(
                encryptedBytes,
                0,
                encryptedBytes.Length);

                result = Encoding.UTF8.GetString(decryptedBytes);
            }

            return result;
        }

        /// <summary>
        /// Формируем ошибку
        /// </summary>
        /// <param name="errorCode">Код ошибки</param>
        /// <param name="errorMessage">Сообщение ошибки</param>
        /// <returns>DTO с ошибкой</returns>
        private static ResponseModel CreateErrorResponse(
            ErrorCode errorCode,
            string errorMessage = ""
            )
        {
            return new ResponseModel
            {
                IsError = 1,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Получаем Email адреса
        /// </summary>
        /// <param name="page">Код страницы</param>
        /// <returns>Коллекция адресов</returns>
        private static List<string> ExtractEmails(string page)
        {
            return EmailRegex
                .Matches(page)
                .Select(match => match.Value)
                .ToList();
        }

        /// <summary>
        /// Внутри по пунктам расписал что и как делается
        /// Пункт 7 размазан по всему проекту по понятным причинам
        /// </summary>
        /// <param name="input">Входные данные (DTO)</param>
        /// <returns>DTO согласно условиям задачи</returns>
        public async Task<ResponseModel> ProcessAsync(InputModel input)
        {
            // Пункт 1
            // Выполняем валидацию входных данных с использованием FluentValidation
            // Если я правильно понял Пункт 11 в задаче на оформление и деплой,
            // то здесь надо обосновать почему я не использовал ассинхронную версию валидации.
            // У меня правила валидации полностью синхронные:
            // RuleFor(x => x.Selector).NotEmpty();
            // там нет какого-то I/O, ожидания, следовательно в этом месте нет смысла
            // использовать ValidateAsync()
            var validationResult = _validator.Validate(input);
            // Если валидация не прошла,
            // собираем все ошибки валидации в одно сообщение
            if (!validationResult.IsValid)
            {
                var errorMessage = "";

                foreach (var error in validationResult.Errors)
                {
                    errorMessage += $"{error.ErrorMessage}\n";
                }

                return CreateErrorResponse(
                    ErrorCode.ValidationError,
                    errorMessage
                    );
            }

            try
            {
                // Пункт 2
                // Конвертируем Base64 url в байтовый массив
                byte[] urlBytes;
                try
                {
                    urlBytes = DecodeBase64(input.UrlB64!);
                }
                catch (FormatException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.InvalidUrlBase64,
                        ex.Message
                        );
                }

                // Конвертируем Base64 page в байтовый массив
                byte[] pageBytes;
                try
                {
                    pageBytes = DecodeBase64(input.PageB64!);
                }
                catch (FormatException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.InvalidPageBase64,
                        ex.Message
                        );
                }

                // Декодируем байтовые массивы в строки
                var url = Encoding.UTF8.GetString(urlBytes);
                var page = Encoding.UTF8.GetString(pageBytes);

                // Пункт 3.1
                // Парсим page в DOM
                var document = await ParsePageToDomAsync(page);

                // Пункт 3.2
                // Получаем все элементы по селектору
                var elementsBySelector = document.QuerySelectorAll(input.Selector!).ToList();

                // Пункт 3.3
                // Определяем кол-во элементов
                var elementsCount = elementsBySelector.Count;

                // Пункт 3.4, 3.5
                // Получаем элементы удовлетворящие атрибуту вводных данных
                // Формируем лист со значениями этих атрибутов
                var elementsAttrList = elementsBySelector
                    .Select(element => element.GetAttribute(input.Attribute!))
                    .ToList();

                // Пункт 4
                // Подготавливаем данные для сохранения в бд
                var entities = ExtractElements(
                    elementsBySelector,
                    input.Attribute!
                    );

                
                // Пункт 5
                // Получаем emails из page, формируем List
                var emailsList = ExtractEmails(page);

                // Пункт 6
                // Расшифровываем текст
                byte[] encryptedBytes;

                try
                {
                    encryptedBytes = DecodeBase64(input.EncryptedTextBytesB64!);
                }
                catch (FormatException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.InvalidEncryptedTextBase64,
                        ex.Message
                        );
                }

                byte[] keyBytes;

                try
                {
                    keyBytes = DecodeBase64(input.KeyBytesB64!);
                }
                catch (FormatException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.InvalidKeyBase64,
                        ex.Message
                        );
                }

                string decryptedPlainText;

                // На тот случай,
                // если параметры шифрования не позволят выполнить расшифровку
                try
                {
                    decryptedPlainText = DecryptText(
                        encryptedBytes,
                        keyBytes
                        );
                }
                catch (CryptographicException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.DecryptionError,
                        ex.Message
                        );
                }

                // Сохраняем данные только сейчас, а не в момент создания entities,
                // если выше что-то выкинет ошибку данные будут сохранены,
                // а итоговый результат вернет error.
                // Здесь используем именно ассинхронную версию сохранения данных,
                // тк I/O операция, чтоб не блокировать поток ожиданием
                try
                {
                    await _elementRepository.AddRangeAsync(entities);
                }
                catch (NpgsqlException ex)
                {
                    return CreateErrorResponse(
                        ErrorCode.DatabaseError,
                        ex.Message
                        );
                }

                return new ResponseModel
                {
                    IsError = 0,
                    ElementsCount = elementsCount,
                    EmailsCount = emailsList.Count,
                    Url = url,
                    DecryptedPlainText = decryptedPlainText,
                    ElementsAttrList = elementsAttrList,
                    EmailsList = emailsList,
                    ErrorCode = ErrorCode.None,
                    ErrorMessage = string.Empty,
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    ErrorCode.UnknownError,
                    ex.Message
                    );
            }
        }
    }
}
