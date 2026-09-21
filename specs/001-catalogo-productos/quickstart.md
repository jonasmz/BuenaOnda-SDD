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
| 13 | Crear un producto sin imagen | Se acepta | Cuestión 9 (provisional) |

## Resultado esperado

Los escenarios 1 a 13 pasan y los productos actuales de `system_requirements.txt` §9 se pueden representar según la tabla de verificación de data-model.md.
