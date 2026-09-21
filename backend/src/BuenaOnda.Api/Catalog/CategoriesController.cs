using BuenaOnda.Application.Catalog.Categories;
using Microsoft.AspNetCore.Mvc;

namespace BuenaOnda.Api.Catalog;

public sealed record CategoryRequest(string? Name, string? Description);

public sealed class CategoriesController : CatalogControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<CategoryView>> List(
        [FromServices] ListCategories useCase, [FromQuery] bool includeInactive = false, CancellationToken ct = default) =>
        await useCase.ExecuteAsync(includeInactive, ct);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateCategory useCase, CategoryRequest request, CancellationToken ct)
    {
        var view = await useCase.ExecuteAsync(request.Name, request.Description, ct);
        return CreatedAtAction(nameof(Get), new { id = view.Id }, view);
    }

    [HttpGet("{id:guid}")]
    public async Task<CategoryView> Get([FromServices] GetCategory useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPut("{id:guid}")]
    public async Task<CategoryView> Update(
        [FromServices] UpdateCategory useCase, Guid id, CategoryRequest request, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, request.Name, request.Description, ct);

    [HttpPost("{id:guid}/deactivate")]
    public async Task<CategoryView> Deactivate([FromServices] DeactivateCategory useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);

    [HttpPost("{id:guid}/reactivate")]
    public async Task<CategoryView> Reactivate([FromServices] ReactivateCategory useCase, Guid id, CancellationToken ct) =>
        await useCase.ExecuteAsync(id, ct);
}
