# Contrato: API administrativa del catálogo

Superficie administrativa única de esta feature. Todas las rutas cuelgan de `/api/admin/catalog`. Formato JSON; errores como ProblemDetails. La autenticación y autorización las aplicará la feature de usuarios y roles sobre este prefijo; esta feature no las define. No existe superficie pública en esta feature.

Tipos: `id` es un identificador único (GUID); `price` es un decimal no negativo. Los nombres se comparan sin distinguir mayúsculas ni espacios de borde.

## Categorías

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `GET /categories?includeInactive=false` | Lista categorías | FR-002 |
| `POST /categories` | Crea una categoría | FR-001, FR-020 |
| `GET /categories/{id}` | Consulta una categoría | FR-002 |
| `PUT /categories/{id}` | Modifica nombre y descripción | FR-003, FR-020 |
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
| `PUT /products/{id}` | Modifica información comercial (nombre, descripción, imagen, categoría) | FR-006, FR-016 |
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
    {
      "values": { "presentación": "500 ml", "sabor": "pomelo" },
      "price": 0,
      "isMarkedAvailable": true,
      "description": null,
      "imageUrl": null
    }
  ]
}
```

- `characteristics` vacío o ausente: el producto tiene exactamente una opción, con `values` vacío.
- `isMarkedAvailable` es obligatorio en cada opción; `description` e `imageUrl` son opcionales.

Respuesta de consulta:

```json
{
  "id": "…", "name": "…", "description": null, "imageUrl": null, "hasImage": false,
  "categoryId": "…", "isActive": true,
  "isAvailable": true, "isVisibleToPublic": false,
  "characteristics": [{ "id": "…", "name": "…" }],
  "options": [{
    "id": "…", "values": {}, "price": 0, "description": null, "imageUrl": null,
    "isMarkedAvailable": true, "isActive": true, "isAvailable": true,
    "isVisibleToPublic": false
  }]
}
```

`isAvailable` e `isVisibleToPublic` (producto y opción) son indicadores derivados de solo lectura (data-model.md); `isMarkedAvailable` es la marca manual.

## Variación y opciones de un producto

| Método y ruta | Descripción | FR |
|---------------|-------------|----|
| `POST /products/{id}/options` | Agrega una opción comercializable | FR-008, FR-010 |
| `PUT /products/{id}/options/{optionId}` | Modifica valores, precio, descripción e imagen de una opción | FR-012, FR-014 |
| `PUT /products/{id}/options/{optionId}/availability` | Fija la marca manual de disponibilidad: `{ "isMarkedAvailable": bool }` | FR-015 |
| `POST /products/{id}/options/{optionId}/deactivate` | Da de baja una opción (reversible) | FR-021 |
| `POST /products/{id}/options/{optionId}/reactivate` | Reactiva una opción | FR-021 |
| `DELETE /products/{id}/options/{optionId}` | Elimina definitivamente una opción no referenciada que no sea la última del producto | FR-021 |
| `POST /products/{id}/characteristics` | Agrega una característica; incluye el valor para cada opción existente | FR-009, FR-010 |
| `DELETE /products/{id}/characteristics/{characteristicId}` | Quita una característica si las opciones siguen siendo distinguibles | FR-009 |

Cuerpo de opción en `POST`: `{ "values": { "<característica>": "<valor>" }, "price": number, "isMarkedAvailable": bool, "description": string|null, "imageUrl": string|null }`. El cuerpo del `PUT` es el mismo sin `isMarkedAvailable`: la marca manual solo se modifica con `PUT …/availability`.

Cuerpo de nueva característica: `{ "name": string, "valuesForExistingOptions": { "<optionId>": "<valor>" } }`.

No existe operación para eliminar productos ni categorías. Las opciones solo se eliminan con `DELETE` bajo las condiciones de FR-021; si están referenciadas se responde 409 y se debe usar la baja.

## Errores

| Estado | Cuándo |
|--------|--------|
| 400 | Datos ausentes o inválidos (nombre vacío, precio negativo, valores incompletos o de más, marca de disponibilidad ausente) |
| 404 | El recurso no existe |
| 409 | Eliminar una opción referenciada o la última opción de un producto; valores de variación de una opción ya existente, aunque esté inactiva; nombre de categoría repetido; nombre de producto repetido en su categoría; opción indistinguible de otra del mismo producto; característica repetida; categoría inactiva al asignar producto; modificación de un producto inactivo |

## Consumidores futuros

POS, recetas, inventario y catálogo público seleccionan un producto o una opción por su `id` estable (FR-019) y pueden leer los indicadores `isAvailable` e `isVisibleToPublic`. Los contratos que expongan cada uno pertenecen a sus propias features.
