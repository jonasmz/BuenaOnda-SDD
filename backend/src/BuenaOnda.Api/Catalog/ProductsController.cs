using BuenaOnda.Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace BuenaOnda.Api.Catalog;

public sealed record OptionRequest(decimal? Price, bool? IsMarkedAvailable, string? Description, string? ImageUrl)
{
    public OptionInput ToInput() => new(Price, IsMarkedAvailable, Description, ImageUrl);
}

public sealed record CreateProductRequest(
    string? Name, string? Description, string? ImageUrl, Guid? CategoryId, IReadOnlyList<OptionRequest>? Options);

public sealed record UpdateProductRequest(string? Name, string? Description, string? ImageUrl, Guid? CategoryId);

public sealed class ProductsController : CatalogControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ProductView>> List(
        [FromServices] ListProducts useCase, [FromQuery] Guid? categoryId, [FromQuery] bool includeInactive = false,
        CancellationToken ct = default) =>
        await useCase.ExecuteAsync(categoryId, includeInactive, ct);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateProduct useCase, CreateProductRequest request, CancellationToken ct)
    {
        // Sin características hay exactamente una opción (invariante 1).
        var view = await useCase.ExecuteAsync(
            request.Name, request.Description, request.ImageUrl, request.CategoryId,
            request.Options is { Count: 1 } ? request.Options[0].ToInput() : null, ct);
        return CreatedAtAction(nameof(Get), new { id = view.Id }, view);
    }

    [HttpGet("{id:guid}")]
    public async Task<ProductView> Get([FromServices] GetProduct useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPut("{id:guid}")]
    public async Task<ProductView> Update(
        [FromServices] UpdateProduct useCase, Guid id, UpdateProductRequest request, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, request.Name, request.Description, request.ImageUrl, request.CategoryId, ct);
}
