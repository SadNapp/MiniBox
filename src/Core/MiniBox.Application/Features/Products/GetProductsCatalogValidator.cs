using FluentValidation;
using MiniBox.Application.Features.Products.Queries.GetProductsCatalog;

namespace MiniBox.Application.Features.Products.Queries.GetProductsCatalog;

public sealed class GetProductsCatalogValidator : AbstractValidator<GetProductsCatalogQuery>
{
    public GetProductsCatalogValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(120);
        RuleFor(query => query.AttributeKey).MaximumLength(80);
        RuleFor(query => query.AttributeValue).MaximumLength(160);
        RuleFor(query => query.Category).MaximumLength(100);
        RuleFor(query => query.MinPrice).GreaterThanOrEqualTo(0).When(query => query.MinPrice.HasValue);
        RuleFor(query => query.MaxPrice).GreaterThanOrEqualTo(0).When(query => query.MaxPrice.HasValue);
        RuleFor(query => query).Must(query => !query.MinPrice.HasValue || !query.MaxPrice.HasValue || query.MinPrice <= query.MaxPrice)
            .WithMessage("Мінімальна ціна не може перевищувати максимальну.");
        RuleFor(query => query.SortBy).Must(value => string.IsNullOrWhiteSpace(value) || value is "price" or "name")
            .WithMessage("Сортування доступне за ціною або назвою.");
        RuleFor(query => query.SortDirection).Must(value => string.IsNullOrWhiteSpace(value) || value is "asc" or "desc")
            .WithMessage("Напрямок сортування має бути asc або desc.");
        RuleFor(query => query).Must(query => string.IsNullOrWhiteSpace(query.AttributeKey) == string.IsNullOrWhiteSpace(query.AttributeValue))
            .WithMessage("Фільтр атрибутів має містити і назву, і значення.");
    }
}
