using FluentValidation;
using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Middleware;

namespace MinimalApiApp.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    private readonly IProductRepository _productRepository;

    public ProductValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        When(x => x.Id != 0, () =>
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0")
                .MustAsync(BeUniqueId)
                .WithMessage("Product ID already exists");
        });

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than 0");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Product stock must be greater than or equal to 0");
    }

    private async Task<bool> BeUniqueId(Product product, int id, CancellationToken cancellationToken)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            return false;
        }
        catch (NotFoundException)
        {
            return true;
        }
    }
}
