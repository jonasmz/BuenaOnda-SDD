using BuenaOnda.Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace BuenaOnda.Api.Catalog;

public sealed record CreateProductRequest(
    string? Name, string? Description, string? ImageUrl, Guid? CategoryId, decimal? Price, bool? IsMarkedAvailable);

public sealed record UpdateProductRequest(string? Name, string? Description, string? ImageUrl, Guid? CategoryId, decimal? Price);

public sealed record AvailabilityRequest(bool? IsMarkedAvailable);

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
        var view = await useCase.ExecuteAsync(
            request.Name, request.Description, request.ImageUrl, request.CategoryId, request.Price, request.IsMarkedAvailable, ct);
        return CreatedAtAction(nameof(Get), new { id = view.Id }, view);
    }

    [HttpGet("{id:guid}")]
    public async Task<ProductView> Get([FromServices] GetProduct useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPut("{id:guid}")]
    public async Task<ProductView> Update(
        [FromServices] UpdateProduct useCase, Guid id, UpdateProductRequest request, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, request.Name, request.Description, request.ImageUrl, request.CategoryId, request.Price, ct);

    /// <summary>Fija la marca manual de disponibilidad (FR-010).</summary>
    [HttpPut("{id:guid}/availability")]
    public async Task<ProductView> SetAvailability(
        [FromServices] SetProductAvailability useCase, Guid id, AvailabilityRequest request, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, request.IsMarkedAvailable, ct);

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ProductView> Deactivate([FromServices] DeactivateProduct useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ProductView> Reactivate([FromServices] ReactivateProduct useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);
}
