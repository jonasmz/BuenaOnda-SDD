using Microsoft.AspNetCore.Mvc;

namespace BuenaOnda.Api.Catalog;

/// <summary>
/// Base de los controladores administrativos del catálogo. Todas las rutas cuelgan de
/// <c>/api/admin/catalog/{controller}</c>, el punto único donde la feature de usuarios y roles aplicará la
/// protección (excepción transitoria del Principio IV, plan.md).
/// </summary>
[ApiController]
[Route("api/admin/catalog/[controller]")]
public abstract class CatalogControllerBase : ControllerBase;
