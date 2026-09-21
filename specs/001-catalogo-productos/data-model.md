# Modelo de datos: Gestión del catálogo de productos

Modelo conceptual para el plan; el mapeo físico lo define la implementación. Cada entidad traza al requerimiento de origen (Principio IX). El esquema SQL de referencia no se usa. El catálogo es plano: no existen variantes, opciones ni características de variación.

## Entidades

### Categoría

Origen: `system_requirements.txt` §2 y §10; spec FR-001 a FR-004, FR-013, FR-015.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema |
| Nombre | Nombre de la categoría | Obligatorio, no vacío; único entre categorías (sin distinguir mayúsculas ni espacios de borde) |
| Descripción | Texto opcional | Opcional |
| Activa | Estado de vigencia | Se puede desactivar y reactivar; no se elimina |

Una categoría sin productos es válida.

### Producto

Origen: §2, §9 y §10; spec FR-004 a FR-015.

| Atributo | Descripción | Reglas |
|----------|-------------|--------|
| Id | Identificador único e inmutable | Generado por el sistema; es la referencia para los consumidores (FR-014) |
| Nombre | Nombre comercial (incluye la presentación o variedad, por ejemplo "Papas fritas grandes") | Obligatorio, no vacío; único dentro de su categoría |
| Descripción | Texto | Opcional |
| Imagen | Referencia de imagen ilustrativa (URL) | Opcional; sin imagen el producto no es visible al público |
| Precio | Precio comercial del producto | Obligatorio; decimal no negativo; moneda única del establecimiento; sin historial |
| Disponibilidad manual | Marca disponible / no disponible | Obligatoria al crear el producto; la fija el usuario administrativo; no cambia con la baja ni con la reactivación |
| Categoría | Exactamente una categoría | Obligatoria; debe estar activa al asignarla; el nombre no puede repetirse en la categoría destino |
| Activo | Estado de vigencia | Se puede desactivar y reactivar; no se elimina |

Relación: pertenece a una categoría. No tiene entidades hijas.

## Indicadores derivados (se calculan al leer; no se almacenan)

| Indicador | Regla |
|-----------|-------|
| Disponibilidad efectiva del producto | Disponibilidad manual, producto activo y categoría activa |
| Visibilidad pública del producto | Producto activo, categoría activa y producto con imagen |

## Invariantes

1. Un producto pertenece a exactamente una categoría y no puede asignarse a una categoría inactiva.
2. Los nombres de categoría son únicos entre categorías; los de producto, únicos dentro de su categoría (comparación sin distinguir mayúsculas ni espacios de borde).
3. El precio de un producto es un decimal no negativo.
4. Los identificadores de categoría y producto no cambian al editar.
5. Un producto inactivo, o de una categoría inactiva, conserva su información; no admite modificarse (información, precio, marca de disponibilidad, categoría) hasta reactivarlo. Sí se puede consultar y reactivar.
6. La baja no altera la disponibilidad manual del producto.
7. Categorías y productos no se eliminan.

## Transiciones de estado

- Categoría: Activa ⇄ Inactiva.
- Producto: Activo ⇄ Inactivo; la marca manual pasa libremente entre disponible y no disponible mientras el producto y su categoría estén activos.
- No existen otros estados ni eliminación definitiva.

## Trazabilidad requisito → modelo

| FR | Elemento del modelo |
|----|---------------------|
| FR-001 a FR-003 | Categoría |
| FR-004, FR-015 | Producto.Categoría (exactamente una); invariantes 1 y 2 |
| FR-005, FR-006 | Producto |
| FR-007, FR-012 | Ausencia de variantes, tipos y campos por producto |
| FR-008, FR-009 | Producto.Precio, sin historial; invariante 3 |
| FR-010 | Producto.Disponibilidad manual; indicadores derivados |
| FR-011 | Producto.Imagen; visibilidad pública derivada |
| FR-013 | Activa / Activo; invariantes 5 a 7; indicadores derivados |
| FR-014 | Id inmutable de producto |

## Escenario de extensibilidad (Principio VIII)

Se comprueba sin modificar ninguna entidad, atributo ni invariante:

1. **Categoría de tragos**: se crea una categoría "Tragos" y un producto "Fernet con cola" con su precio. Usa las mismas entidades que las papas fritas.
2. **Producto de modalidad elaborada** (hamburguesa): se registra como "Hamburguesa completa". Su futura receta se asociará desde la especificación de recetas, referenciando el Id del producto; el catálogo no cambia.
3. **Producto de stock directo** (pizza congelada, gaseosa): se registra igual que el anterior. Su control de existencias se definirá en inventario y referenciará el Id del producto.
4. **Nueva presentación**: una gaseosa en un tamaño nuevo es un producto nuevo; no requiere cambios en el modelo.

El catálogo no contiene atributos de inventario ni de recetas, por lo que ambas modalidades se representan idénticamente en él. La disponibilidad es una marca manual, no un concepto de inventario.

## Verificación con los productos actuales

| Producto | Productos planos |
|----------|------------------|
| Papas fritas | "Papas fritas chicas", "Papas fritas grandes" |
| Milanesa / hamburguesa | "Milanesa simple", "Milanesa completa", "Hamburguesa simple", "Hamburguesa completa" |
| Pizza | un producto por variedad ("Pizza muzzarella", …) |
| Gaseosa, agua mineral, cerveza | un producto por marca y presentación ("Coca-Cola 500 ml", "Sprite 1,5 litros", "Agua mineral 500 ml", "Cerveza 1 litro") |
| Agua saborizada | un producto por sabor y presentación ("Agua saborizada pomelo 500 ml") |
