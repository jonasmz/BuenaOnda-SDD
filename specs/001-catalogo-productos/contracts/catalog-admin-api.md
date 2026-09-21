# Contrato: API administrativa del catálogo

Superficie administrativa única de esta feature. Todas las rutas cuelgan de `/api/admin/catalog`. Formato JSON; errores como ProblemDetails. La autenticación y autorización las aplicará la feature de usuarios y roles sobre este prefijo; esta feature no las define. No existe superficie pública en esta feature.

Tipos: `id` es un identificador único (GUID); `price` es un decimal no negativo.

## Categorías

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `GET /categories?includeInactive=false` | Lista categorías | FR-002 |
| `POST /categories` | Crea una categoría | FR-001 |
| `GET /categories/{id}` | Consulta una categoría | FR-002 |
| `PUT /categories/{id}` | Modifica nombre y descripción | FR-003 |
| `POST /categories/{id}/deactivate` | Da de baja (reversible) | FR-018 |
| `POST /categories/{id}/reactivate` | Reactiva | FR-018 |

Cuerpo de creación y modificación: `{ "name": string, "description": string|null }`.

Respuesta: `{ "id", "name", "description", "isActive" }`.

## Productos

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `GET /products?categoryId=&includeInactive=false` | Lista productos | FR-006 |
| `POST /products` | Crea un producto con su estructura de variación | FR-005, FR-008 |
| `GET /products/{id}` | Consulta un producto con sus características y opciones | FR-006, FR-011 |
| `PUT /products/{id}` | Modifica información comercial (nombre, descripción, imagen, categoría) | FR-006 |
| `POST /products/{id}/deactivate` | Da de baja (reversible) | FR-018 |
| `POST /products/{id}/reactivate` | Reactiva | FR-018 |

Creación:

```json
{
  "name": "Agua saborizada",
  "description": null,
  "imageUrl": null,
  "categoryId": "…",
  "characteristics": ["presentación", "sabor"],
  "options": [
    { "values": { "presentación": "500 ml", "sabor": "pomelo" }, "price": 0 }
  ]
}
```

- `characteristics` vacío o ausente: el producto tiene exactamente una opción, con `values` vacío.
- Respuesta de consulta: `{ id, name, description, imageUrl, categoryId, isActive, characteristics: [{id, name}], options: [{ id, values, price }] }`.

## Variación y opciones de un producto

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `POST /products/{id}/options` | Agrega una opción comercializable | FR-008, FR-010 |
| `PUT /products/{id}/options/{optionId}` | Modifica valores y precio de una opción | FR-012, FR-014 |
| `POST /products/{id}/characteristics` | Agrega una característica; incluye el valor para cada opción existente | FR-009, FR-010 |
| `DELETE /products/{id}/characteristics/{characteristicId}` | Quita una característica si las opciones siguen siendo distinguibles | FR-009 |

Cuerpo de opción: `{ "values": { "<característica>": "<valor>" }, "price": number }`.

Cuerpo de nueva característica: `{ "name": string, "valuesForExistingOptions": { "<optionId>": "<valor>" } }`.

No existe operación para eliminar productos, categorías ni opciones.

## Errores

| Estado | Cuándo |
|--------|--------|
| 400 | Datos ausentes o inválidos (nombre vacío, precio negativo, valores incompletos o de más) |
| 404 | El recurso no existe |
| 409 | Opción indistinguible de otra del mismo producto; característica repetida; categoría inactiva al asignar producto; modificación de la estructura de un producto inactivo |

## Consumidores futuros

POS, recetas, inventario y catálogo público seleccionan un producto o una opción por su `id` estable (FR-019). Los contratos que expongan cada uno pertenecen a sus propias features.
