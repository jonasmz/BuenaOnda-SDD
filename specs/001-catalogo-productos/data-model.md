# Modelo de datos: Gestión del catálogo de productos

Modelo conceptual para el plan; el mapeo físico lo define la implementación. Cada entidad traza al requerimiento de origen (Principio IX). El esquema SQL de referencia no se usa.

## Entidades

### Categoría

Origen: `system_requirements.txt` §2 y §10; spec FR-001 a FR-020, FR-018.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema |
| Nombre | Nombre de la categoría | Obligatorio, no vacío; único entre categorías (sin distinguir mayúsculas ni espacios de borde) |
| Descripción | Texto opcional | Opcional |
| Activa | Estado de vigencia | Se puede desactivar y reactivar; no se elimina |

Una categoría sin productos es válida.

### Producto

Origen: §2, §9 y §10; spec FR-004, FR-020, FR-005 a FR-008, FR-016, FR-018.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema |
| Nombre | Nombre comercial | Obligatorio, no vacío; único dentro de su categoría |
| Descripción | Texto | Opcional |
| Imagen | Referencia de imagen ilustrativa (URL) | Opcional; sin imagen el producto no es visible al público |
| Categoría | Exactamente una categoría | Obligatoria; debe estar activa al asignarla; el nombre no puede repetirse en la categoría destino |
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

Origen: §4 (variantes del producto en el POS), §10; spec FR-008 a FR-013, FR-015, FR-019, FR-021.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema; es la referencia para los consumidores |
| Precio | Precio comercial de la opción | Obligatorio; decimal no negativo; moneda única del establecimiento |
| Valores | Un valor de texto por cada característica del producto | Deben existir valores para todas las características y ninguno adicional |
| Disponibilidad manual | Marca disponible / no disponible | Obligatoria al crear la opción; la fija el usuario administrativo; no cambia con la baja ni con la reactivación |
| Activa | Estado de vigencia | Se puede dar de baja y reactivar; solo se elimina definitivamente si nunca fue referenciada y no es la última opción del producto (FR-021) |
| Descripción | Texto propio de la opción | Opcional |
| Imagen | Referencia de imagen propia (URL) | Opcional; informativa, no condiciona la visibilidad del producto |

Relaciones: pertenece a un producto; sus valores referencian características de ese mismo producto.

## Indicadores derivados (se calculan al leer; no se almacenan)

| Indicador | Regla |
|-----------|-------|
| Disponibilidad efectiva de la opción | Disponibilidad manual, opción activa, producto activo y categoría activa |
| Disponibilidad del producto | Alguna de sus opciones con disponibilidad efectiva |
| Visibilidad pública del producto | Producto activo, categoría activa y producto con imagen |
| Visibilidad pública de la opción | Opción activa y visibilidad pública del producto |

## Invariantes

1. Un producto sin características tiene exactamente una opción, con valores vacíos (FR-008).
2. Un producto con características tiene al menos una opción, y cada opción define un valor no vacío para cada característica (FR-010).
3. Dos opciones del mismo producto no comparten el mismo conjunto de valores, sin distinguir mayúsculas ni espacios de borde (FR-011).
4. Los nombres de característica son únicos dentro del producto.
5. Agregar una característica a un producto con opciones exige aportar en la misma operación su valor para cada opción existente; quitarla solo es válido si las opciones restantes siguen siendo distinguibles.
6. Los identificadores de categoría, producto, característica y opción no cambian al editar (FR-018, FR-019).
7. Un producto inactivo, o de una categoría inactiva, conserva su información; no admite modificar su estructura, opciones, precios ni marcas hasta reactivarlo, y no se puede asignar un producto a una categoría inactiva.
8. Los nombres de categoría son únicos entre categorías; los de producto, únicos dentro de su categoría (FR-020).
9. La baja no altera la disponibilidad manual de las opciones (FR-018).
10. Un producto conserva siempre al menos una opción: no se puede eliminar la última (FR-021). Las opciones inactivas cuentan como opciones.
11. Una opción solo se elimina definitivamente si ninguna funcionalidad consumidora la ha referenciado (comprobación mediante el puerto de referencias); si fue referenciada, solo se da de baja (FR-021).
12. Los valores de variación de una opción inactiva siguen reservados mientras la opción exista (invariante 3).

## Transiciones de estado

- Categoría: Activa ⇄ Inactiva.
- Producto: Activo ⇄ Inactivo.
- Opción: Activa ⇄ Inactiva; la marca manual pasa libremente entre disponible y no disponible.
- Opción: puede eliminarse definitivamente solo en las condiciones del invariante 11.
- Categorías y productos no se eliminan; no existen otros estados.

## Trazabilidad requisito → modelo

| FR | Elemento del modelo |
|----|---------------------|
| FR-001 a FR-003 | Categoría |
| FR-004, FR-020 | Producto.Categoría (exactamente una); invariante 8 |
| FR-005, FR-006 | Producto |
| FR-007, FR-009, FR-010 | Característica de variación y valores como datos |
| FR-008, FR-011, FR-012 | Opción comercializable e invariantes 1 a 5 |
| FR-013, FR-014 | Opción.Precio, sin historial |
| FR-015 | Opción.Disponibilidad manual; indicadores derivados |
| FR-016 | Producto.Imagen; visibilidad pública derivada |
| FR-017 | Ausencia de tipos y campos por producto |
| FR-018 | Activa / Activo; invariantes 7 y 9; indicadores derivados |
| FR-021 | Opción.Activa; invariantes 10 a 12 |
| FR-019 | Ids inmutables de producto y opción |

## Escenario de extensibilidad (Principio VIII)

Se comprueba sin modificar ninguna entidad, atributo ni invariante:

1. **Nueva característica de variación**: un producto nuevo define una característica que hoy nadie usa (por ejemplo "graduación"). Se crea como un nombre y valores de texto; no requiere migración ni cambio de código.
2. **Categoría de tragos**: se crea una categoría "Tragos" y un producto "Fernet con cola" con una característica "tamaño". Usa las mismas entidades que las papas fritas.
3. **Producto de modalidad elaborada** (hamburguesa): se registra con la característica "modalidad" (simple, completa). Su futura receta se asociará desde la especificación de recetas, referenciando el Id del producto o de la opción; el catálogo no cambia.
4. **Producto de stock directo** (pizza congelada, gaseosa): se registra igual que el anterior. Su control de existencias se definirá en inventario y referenciará el Id de la opción.

El catálogo no contiene atributos de inventario ni de recetas, por lo que ambas modalidades se representan idénticamente en él. La disponibilidad es una marca manual, no un concepto de inventario.

## Verificación con los productos actuales

| Producto | Características | Opciones |
|----------|------------------|----------|
| Papas fritas | tamaño | chica, grande |
| Milanesa / hamburguesa | modalidad | simple, completa |
| Pizza | variedad | una opción por variedad |
| Gaseosa (un producto por marca: Coca-Cola, Sprite…), agua mineral, cerveza | presentación | una por presentación |
| Agua saborizada | presentación y sabor | una por combinación |
