namespace PackControl.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductTemplateDto>> ListAsync(CancellationToken cancellationToken);
    Task<ProductTemplateDto> CreateAsync(CreateProductTemplateRequest request, CancellationToken cancellationToken);
    Task<ProductTemplateDto?> UpdateAsync(Guid productTemplateId, UpdateProductTemplateRequest request, CancellationToken cancellationToken);
}
