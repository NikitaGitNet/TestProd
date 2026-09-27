using FluentValidation;

namespace TestProd.Models
{
    public class InputModelValidator : AbstractValidator<InputModel>
    {
        public InputModelValidator()
        {
            RuleFor(x => x.Selector)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Селектор обязателен");

            RuleFor(x => x.Attribute)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Атрибут обязателен");

            RuleFor(x => x.UrlB64)
                .NotEmpty()
                .WithMessage("URL обязателен");

            RuleFor(x => x.EncryptedTextBytesB64)
                .NotEmpty()
                .WithMessage("Зашифрованный текст обязателен");

            RuleFor(x => x.KeyBytesB64)
                .NotEmpty()
                .WithMessage("Ключ обязателен");

            RuleFor(x => x.PageB64)
                .NotEmpty()
                .WithMessage("Page обязателен");
        }
    }
}
