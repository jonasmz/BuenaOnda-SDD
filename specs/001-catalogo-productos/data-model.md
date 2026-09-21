# Modelo de datos: Gestión del catálogo de productos

Modelo conceptual para el plan; el mapeo físico lo define la implementación. Cada entidad traza al requerimiento de origen (Principio IX). El esquema SQL de referencia no se usa.

## Entidades

### Categoría

Origen: `system_requirements.txt` §2 y §10; spec FR-001 a FR-004, FR-018.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema |
| Nombre | Nombre de la categoría | Obligatorio, no vacío |
| Descripción | Texto opcional | Opcional |
| Activa | Estado de vigencia | Se puede desactivar y reactivar; no se elimina |

Relaciones: una categoría tiene cero o más productos. Una categoría sin productos es válida.

### Producto

Origen: §2, §9 y §10; spec FR-005 a FR-008, FR-018.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema |
| Nombre | Nombre comercial | Obligatorio, no vacío |
| Descripción | Texto | Opcional |
| Imagen | Referencia de imagen ilustrativa (URL) | Opcional (Cuestión 9 abierta) |
| Categoría | Exactamente una categoría | Obligatoria; debe ser una categoría activa al asignarla |
| Activo | Estado de vigencia | Se puede desactivar y reactivar; no se elimina |

Relaciones: pertenece a una categoría; contiene cero o más características de variación y una o más opciones comercializables.

### Característica de variación

Origen: §10 (formas de variación); spec FR-009, FR-010.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único | Generado por el sistema |
| Nombre | Nombre de la característica (por ejemplo presentación, sabor) | Único dentro del producto, sin distinguir mayúsculas ni espacios de borde |

Pertenece a un solo producto; no existe catálogo global de características.

### Opción comercializable (variante)

Origen: §4 (variantes del producto en el POS), §10; spec FR-008 a FR-013, FR-019.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema; es la referencia para los consumidores |
| Precio | Precio comercial de la opción | Obligatorio; decimal no negativo; moneda única del establecimiento |
| Valores | Un valor de texto por cada característica del producto | Deben existir valores para todas las características y ninguno adicional |

Relaciones: pertenece a un producto; sus valores referencian características de ese mismo producto.

## Invariantes del agregado Producto

1. Un producto sin características tiene exactamente una opción, con valores vacíos (FR-008).
2. Un producto con características tiene al menos una opción, y cada opción define un valor no vacío para cada característica (FR-010).
3. Dos opciones del mismo producto no comparten el mismo conjunto de valores, sin distinguir mayúsculas ni espacios de borde (FR-011).
4. Los nombres de característica son únicos dentro del producto.
5. Agregar una característica a un producto con opciones exige aportar en la misma operación su valor para cada opción existente; quitarla solo es válido si las opciones restantes siguen siendo distinguibles.
6. Los identificadores de producto, característica y opción no cambian al editar (FR-018, FR-019).
7. Un producto inactivo o de una categoría que se desactiva conserva su información; el catálogo no permite nuevos usos de elementos inactivos (asignar productos a una categoría inactiva, o modificar la estructura y precios de un producto inactivo antes de reactivarlo).

## Transiciones de estado

- Categoría: Activa ⇄ Inactiva.
- Producto: Activo ⇄ Inactivo.
- No existen otros estados ni eliminación.

## Trazabilidad requisito → modelo

| FR | Elemento del modelo |
|----|---------------------|
| FR-001 a FR-003 | Categoría |
| FR-004 | Producto.Categoría (exactamente una) |
| FR-005, FR-006 | Producto |
| FR-007, FR-009, FR-010 | Característica de variación y valores como datos |
| FR-008, FR-011, FR-012 | Opción comercializable e invariantes 1 a 5 |
| FR-013, FR-014 | Opción.Precio, sin historial |
| FR-015 | No se persiste en esta feature (research.md R-10) |
| FR-016 | Producto.Imagen |
| FR-017 | Ausencia de tipos y campos por producto |
| FR-018 | Activa / Activo; invariante 7 |
| FR-019 | Ids inmutables de producto y opción |

## Escenario de extensibilidad (Principio VIII)

Se comprueba sin modificar ninguna entidad, atributo ni invariante:

1. **Nueva característica de variación**: un producto nuevo define una característica que hoy nadie usa (por ejemplo "graduación"). Se crea como un nombre y valores de texto; no requiere migración ni cambio de código.
2. **Categoría de tragos**: se crea una categoría "Tragos" y un producto "Fernet con cola" con una característica "tamaño". Usa las mismas entidades que las papas fritas.
3. **Producto de modalidad elaborada** (hamburguesa): se registra con la característica "modalidad" (simple, completa). Su futura receta se asociará desde la especificación de recetas, referenciando el Id del producto o de la opción; el catálogo no cambia.
4. **Producto de stock directo** (pizza congelada, gaseosa): se registra igual que el anterior. Su control de existencias se definirá en inventario y referenciará el Id de la opción.

El catálogo no contiene atributos de inventario ni de recetas, por lo que ambas modalidades se representan idénticamente en él.

## Verificación con los productos actuales

| Producto | Características | Opciones |
|----------|------------------|----------|
| Papas fritas | tamaño | chica, grande |
| Milanesa / hamburguesa | modalidad | simple, completa |
| Pizza | variedad | una opción por variedad |
| Gaseosa, agua mineral, cerveza | presentación (y marca como característica o como producto, a decidir al cargar el catálogo) | una por presentación |
| Agua saborizada | presentación y sabor | una por combinación |
