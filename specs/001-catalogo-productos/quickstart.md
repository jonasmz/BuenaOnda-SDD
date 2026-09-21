# Quickstart: validación de la gestión del catálogo

Guía para comprobar de extremo a extremo que la feature funciona. Los detalles están en [contracts/catalog-admin-api.md](./contracts/catalog-admin-api.md) y [data-model.md](./data-model.md).

## Prerrequisitos

- El contenedor Docker `psql-17` con PostgreSQL 17 en ejecución.
- .NET SDK 10 y Node.js compatible con Angular 22.
- Migraciones de EF Core aplicadas a la base de desarrollo.

## Arranque

1. Iniciar la API desde `backend/src/BuenaOnda.Api`.
2. Iniciar el frontend administrativo desde `frontend/admin`.
3. Ejecutar las pruebas: la solución de `backend/` y las del proyecto de `frontend/admin`.

## Escenarios de validación

Cada uno se verifica desde la interfaz administrativa o directamente contra la API.

| # | Escenario | Resultado esperado | Spec |
|---|-----------|--------------------|------|
| 1 | Crear las categorías "Comidas" y "Bebidas", consultarlas y modificar una | Aparecen en la consulta con la información actualizada | Historia 1 |
| 2 | Crear "Papas fritas chicas" y "Papas fritas grandes" | Dos productos, cada uno con su precio y disponibilidad, sin relación entre ellos | Escenario 1 |
| 3 | Crear "Hamburguesa simple" y "Hamburguesa completa" | Dos productos independientes | Escenario 2 |
| 4 | Crear una pizza por variedad ("Pizza muzzarella", "Pizza fugazzeta") | Un producto por variedad | Escenario 3 |
| 5 | Crear "Coca-Cola 500 ml", "Coca-Cola 1,5 litros" y luego una presentación nueva | Cada presentación es un producto; la nueva se acepta sin cambios en el sistema | Escenario 4 |
| 6 | Crear "Agua saborizada pomelo 500 ml" y "Agua saborizada manzana 1,5 litros" | Productos independientes | Escenario 5 |
| 7 | Crear una categoría "Tragos" y el producto "Fernet con cola" | Se guarda sin modificar el sistema | Escenario 6 |
| 8 | Modificar el precio de un producto | La consulta devuelve el nuevo precio; no hay historial | FR-009 |
| 9 | Crear un producto con precio negativo o sin precio | Se rechaza con error de validación (400) | FR-008 |
| 10 | Dar de baja y reactivar un producto y una categoría | Conservan su información; no hay operación de eliminación | FR-013 |
| 11 | Asignar un producto a una categoría inactiva | Se rechaza con conflicto (409) | FR-013 |
| 12 | Modificar un producto dado de baja | Se rechaza con conflicto (409) hasta reactivarlo | FR-013 |
| 13 | Crear un producto sin imagen | Se acepta, figura como sin imagen y su visibilidad pública es falsa | FR-011 |
| 14 | Asignarle imagen al producto | Su visibilidad pública pasa a verdadera si está activo y su categoría está activa | FR-011 |
| 15 | Marcar un producto como no disponible y luego como disponible | La consulta refleja cada marca | FR-010 |
| 16 | Dar de baja un producto no disponible y reactivarlo | Durante la baja no es disponible ni visible; al reactivar, su marca manual se conserva | FR-013 |
| 17 | Dar de baja una categoría con productos | Sus productos dejan de ser disponibles y visibles; se conservan sus datos | FR-013 |
| 18 | Crear una categoría con nombre repetido (mayúsculas distintas) | Se rechaza con conflicto (409) | FR-015 |
| 19 | Crear dos productos con el mismo nombre en distintas categorías | Se aceptan; en la misma categoría se rechaza (409) | FR-015 |
| 20 | Mover un producto a una categoría donde ya existe ese nombre | Se rechaza con conflicto (409) | FR-015 |

## Resultado esperado

Los escenarios 1 a 20 pasan y los productos actuales de `system_requirements.txt` §9 se pueden representar según la tabla de verificación de data-model.md.
