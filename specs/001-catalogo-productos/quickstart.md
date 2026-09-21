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
| 2 | Crear "Papas fritas" con característica "tamaño" y opciones chica y grande | El producto lista dos opciones con su propio precio | Escenario 1 |
| 3 | Crear "Hamburguesa" con "modalidad": simple y completa | Dos opciones consultables sin ambigüedad | Escenario 2 |
| 4 | Crear "Pizza" con "variedad" y varias opciones | Una opción por variedad, sin estructura distinta | Escenario 3 |
| 5 | Crear una bebida con presentaciones 500 ml, 750 ml, 1 litro y 1,5 litros; luego agregar una nueva presentación | La nueva presentación se acepta sin cambios en el sistema | Escenario 4 |
| 6 | Crear "Agua saborizada" con "presentación" y "sabor" | Cada combinación es una opción distinta | Escenario 5 |
| 7 | Crear un producto sin características | Tiene una única opción con su precio | FR-008 |
| 8 | Crear una categoría "Tragos" y un producto con una característica nueva | Se guarda sin modificar el sistema | Escenario 6 |
| 9 | Duplicar una opción con los mismos valores en el mismo producto | Se rechaza con conflicto (409) | FR-011 |
| 10 | Modificar el precio de una opción | La consulta devuelve el nuevo precio; no hay historial | FR-014 |
| 11 | Dar de baja y reactivar un producto y una categoría | Conservan su información; no hay operación de eliminación | FR-018 |
| 12 | Asignar un producto a una categoría inactiva | Se rechaza con conflicto (409) | FR-018 |
| 13 | Crear un producto sin imagen | Se acepta, figura como sin imagen y su visibilidad pública es falsa | FR-016 |
| 14 | Asignarle imagen al producto | Su visibilidad pública pasa a verdadera si está activo y su categoría está activa | FR-016 |
| 15 | Marcar una opción como no disponible | Solo esa opción no está disponible; el producto sigue disponible mientras otra lo esté | FR-015 |
| 16 | Marcar no disponibles todas las opciones de un producto | El producto figura no disponible | FR-015 |
| 17 | Dar de baja un producto y reactivarlo | Durante la baja no es disponible ni visible; al reactivar, las marcas manuales de sus opciones se conservan | FR-018 |
| 18 | Dar de baja una categoría con productos | Sus productos dejan de ser disponibles y visibles; se conservan sus datos | FR-018 |
| 19 | Crear una categoría con nombre repetido (mayúsculas distintas) | Se rechaza con conflicto (409) | FR-020 |
| 20 | Crear dos productos con el mismo nombre en distintas categorías | Se aceptan; en la misma categoría se rechaza (409) | FR-020 |
| 21 | Asignar descripción e imagen propias a una opción | Se guardan y no alteran la visibilidad del producto | FR-012 |
| 22 | Eliminar una opción cargada por error que no fue referenciada y no es la última | Se elimina y el producto conserva sus demás opciones | FR-021 |
| 23 | Intentar eliminar la única opción de un producto | Se rechaza con conflicto (409) | FR-021 |
| 24 | Dar de baja una opción y volver a crear una opción con los mismos valores | La opción figura no disponible y su visibilidad pública es falsa; la nueva se rechaza (409); al reactivar la original vuelve a estar disponible según su marca | FR-021, FR-011 |
| 25 | Intentar eliminar una opción referenciada | Se rechaza con conflicto (409); se puede dar de baja. Cubierto solo por prueba automatizada con un doble de `IOptionReferenceChecker`, porque aún no existen consumidores que referencien opciones | FR-021 |

## Resultado esperado

Los escenarios 1 a 25 pasan y los productos actuales de `system_requirements.txt` §9 se pueden representar según la tabla de verificación de data-model.md.
