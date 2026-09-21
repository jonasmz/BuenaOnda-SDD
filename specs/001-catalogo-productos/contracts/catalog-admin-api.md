# Contrato: API administrativa del catálogo

Superficie administrativa única de esta feature. Todas las rutas cuelgan de `/api/admin/catalog`. Formato JSON; errores como ProblemDetails. La autenticación y autorización las aplicará la feature de usuarios y roles sobre este prefijo; esta feature no las define. No existe superficie pública en esta feature.

Tipos: `id` es un identificador único (GUID); `price` es un decimal no negativo. Los nombres se comparan sin distinguir mayúsculas ni espacios de borde. El catálogo es plano: no hay variantes ni opciones.

## Categorías

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `GET /categories?includeInactive=false` | Lista categorías | FR-002 |
| `POST /categories` | Crea una categoría | FR-001, FR-015 |
| `GET /categories/{id}` | Consulta una categoría | FR-002 |
| `PUT /categories/{id}` | Modifica nombre y descripción | FR-003, FR-015 |
| `POST /categories/{id}/deactivate` | Da de baja (reversible) | FR-013 |
| `POST /categories/{id}/reactivate` | Reactiva | FR-013 |

Cuerpo de creación y modificación: `{ "name": string, "description": string|null }`.

Respuesta: `{ "id", "name", "description", "isActive" }`.

## Productos

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `GET /products?categoryId=&includeInactive=false` | Lista productos | FR-006 |
| `POST /products` | Crea un producto | FR-005 |
| `GET /products/{id}` | Consulta un producto | FR-006, FR-014 |
| `PUT /products/{id}` | Modifica información comercial y precio (nombre, descripción, imagen, categoría, precio) | FR-006, FR-009, FR-011 |
| `PUT /products/{id}/availability` | Fija la marca manual de disponibilidad: `{ "isMarkedAvailable": bool }` | FR-010 |
| `POST /products/{id}/deactivate` | Da de baja (reversible) | FR-013 |
| `POST /products/{id}/reactivate` | Reactiva | FR-013 |

Cuerpo de creación:

```json
{
  "name": "Papas fritas grandes",
  "description": null,
  "imageUrl": null,
  "categoryId": "…",
  "price": 1800,
  "isMarkedAvailable": true
}
```

- `name`, `categoryId`, `price` e `isMarkedAvailable` son obligatorios; `description` e `imageUrl` son opcionales.
- El cuerpo del `PUT /products/{id}` es el mismo sin `isMarkedAvailable`: la marca manual solo se modifica con `PUT …/availability`.

Respuesta de consulta:

```json
{
  "id": "…", "name": "…", "description": null, "imageUrl": null, "hasImage": false,
  "categoryId": "…", "price": 1800, "isActive": true,
  "isMarkedAvailable": true, "isAvailable": true, "isVisibleToPublic": false
}
```

`isAvailable` e `isVisibleToPublic` son indicadores derivados de solo lectura (data-model.md); `isMarkedAvailable` es la marca manual.

No existe operación para eliminar productos ni categorías.

## Errores

| Estado | Cuándo |
|--------|--------|
| 400 | Datos ausentes o inválidos (nombre vacío, precio ausente o negativo, marca de disponibilidad ausente, URL de imagen inválida) |
| 404 | El recurso no existe |
| 409 | Nombre de categoría repetido; nombre de producto repetido en su categoría; categoría inactiva al asignar un producto; modificación de un producto inactivo o de una categoría inactiva |

## Consumidores futuros

POS, recetas, inventario y catálogo público seleccionan un producto por su `id` estable (FR-014) y pueden leer los indicadores `isAvailable` e `isVisibleToPublic`. Los contratos que expongan cada uno pertenecen a sus propias features.
