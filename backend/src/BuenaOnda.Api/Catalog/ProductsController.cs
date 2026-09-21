using BuenaOnda.Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace BuenaOnda.Api.Catalog;

public sealed record OptionRequest(
    Dictionary<string, string>? Values, decimal? Price, bool? IsMarkedAvailable, string? Description, string? ImageUrl)
{
    public OptionInput ToInput() => new(Values, Price, IsMarkedAvailable, Description, ImageUrl);
}

public sealed record AddCharacteristicRequest(string? Name, Dictionary<Guid, string>? ValuesForExistingOptions);

public sealed record CreateProductRequest(
    string? Name, string? Description, string? ImageUrl, Guid? CategoryId,
    IReadOnlyList<string>? Characteristics, IReadOnlyList<OptionRequest>? Options);

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
        var view = await useCase.ExecuteAsync(
            request.Name, request.Description, request.ImageUrl, request.CategoryId,
            request.Characteristics, request.Options?.Select(o => o.ToInput()).ToList(), ct);
        return CreatedAtAction(nameof(Get), new { id = view.Id }, view);
    }

    [HttpGet("{id:guid}")]
    public async Task<ProductView> Get([FromServices] GetProduct useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPut("{id:guid}")]
    public async Task<ProductView> Update(
        [FromServices] UpdateProduct useCase, Guid id, UpdateProductRequest request, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, request.Name, request.Description, request.ImageUrl, request.CategoryId, ct);

    [HttpPost("{id:guid}/options")]
    public async Task<IActionResult> AddOption(
        [FromServices] AddOption useCase, Guid id, OptionRequest request, CancellationToken ct)
    {
        var view = await useCase.ExecuteAsync(id, request.ToInput(), ct);
        return CreatedAtAction(nameof(Get), new { id }, view);
    }

    [HttpPost("{id:guid}/characteristics")]
    public async Task<IActionResult> AddCharacteristic(
        [FromServices] AddCharacteristic useCase, Guid id, AddCharacteristicRequest request, CancellationToken ct)
    {
        var view = await useCase.ExecuteAsync(id, request.Name, request.ValuesForExistingOptions, ct);
        return CreatedAtAction(nameof(Get), new { id }, view);
    }

    [HttpDelete("{id:guid}/characteristics/{characteristicId:guid}")]
    public async Task<ProductView> RemoveCharacteristic(
        [FromServices] RemoveCharacteristic useCase, Guid id, Guid characteristicId, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, characteristicId, ct);
}
