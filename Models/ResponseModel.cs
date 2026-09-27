using System.Text.Json.Serialization;

namespace TestProd.Models
{
    public class ResponseModel
    {
        /// <summary>
        /// 0 - нет ошибок, 1 - ошибка
        /// </summary>
        [JsonPropertyName("is_error")]
        public int IsError { get; set; }

        /// <summary>
        /// текстовый код ошибки
        /// </summary>
        [JsonPropertyName("error_code")]
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// для exception.errormessage
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// количество выбранных по селектору элементов
        /// </summary>
        [JsonPropertyName("elements_count")]
        public int ElementsCount { get; set; }

        /// <summary>
        /// количество найденных email
        /// </summary>
        [JsonPropertyName("emails_count")]
        public int EmailsCount { get; set; }

        /// <summary>
        /// URL в открытом виде(раскодированный из base64)
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// текст расшифрованный из полученного шифротекста и ключа
        /// </summary>
        [JsonPropertyName("decrypted_plain_text")]
        public string? DecryptedPlainText { get; set; }

        /// <summary>
        /// список выбранных из обнаруженных элементов атрибутов
        /// </summary>
        [JsonPropertyName("elements_attr_list")]
        public List<string?> ElementsAttrList { get; set; } = new();

        /// <summary>
        /// список email
        /// </summary>
        [JsonPropertyName("emails_list")]
        public List<string> EmailsList { get; set; } = new();
    }
}
