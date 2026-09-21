# Especificación funcional: Gestión del catálogo de productos

**Feature Branch**: `001-catalogo-productos`

**Created**: 2026-09-21

**Status**: Draft

**Input**: Feature "001 — Gestión del catálogo de productos": permitir que un usuario administrativo autorizado gestione la definición comercial del catálogo (categorías, productos, variantes, información comercial y precios).

## Propósito y conceptos

Esta especificación es la fuente funcional de verdad de los conceptos **categoría**, **producto** y **variante** (Requerimientos §10). Otras funcionalidades (POS, recetas, inventario, catálogo público) los consumirán posteriormente; aquí solo se define el catálogo comercial.

Conceptos funcionales usados en este documento:

- **Categoría**: agrupación con la que se organizan los productos.
- **Producto**: bien comercializado por el bar, con su información comercial.
- **Característica de variación**: un eje por el cual las opciones de un producto se distinguen (por ejemplo, presentación, sabor, modalidad). Cada producto define las que necesita; no existe un conjunto global cerrado.
- **Opción comercializable (variante)**: cada alternativa concreta que se puede vender de un producto, identificada por un valor para cada una de sus características de variación.
- **Usuario administrativo autorizado**: actor genérico. Los roles, permisos y la autenticación quedan fuera de esta especificación.

## Clarifications

### Session 2026-09-21

- Q: ¿Un producto puede pertenecer a una sola categoría o a varias categorías a la vez? → A: Exactamente una categoría por producto.
- Q: Cuando un producto tiene variantes, ¿dónde se define el precio? → A: Cada variante tiene su propio precio (un producto sin variantes tiene el suyo).
- Q: ¿Cómo se retira un producto o categoría del catálogo? → A: Solo baja/desactivación reversible; no hay eliminación definitiva.
- Q: ¿Puede existir un producto sin variantes? → A: Sí, con su propio precio.
- Q: Al dar de baja un producto o variante, ¿qué pasa con su uso en el resto del sistema? → A: Se conserva y las referencias existentes siguen válidas; no se puede seleccionar para nuevos usos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Administrar categorías (Priority: P1)

Un usuario administrativo autorizado registra, consulta y modifica las categorías con las que se organiza la oferta del bar.

**Why this priority**: sin categorías no puede organizarse el catálogo ni, más adelante, filtrarse por ellas.

**Independent Test**: se puede probar creando, consultando y modificando categorías sin que existan productos.

**Acceptance Scenarios**:

1. **Given** un catálogo sin categorías, **When** el usuario registra una categoría con su información, **Then** la categoría aparece en la consulta de categorías.
2. **Given** una categoría existente, **When** el usuario modifica su información, **Then** la consulta muestra la información actualizada.
3. **Given** una categoría sin productos, **When** el usuario la consulta, **Then** se muestra sin productos asociados y sin error.

---

### User Story 2 - Registrar productos y su información comercial (Priority: P1)

Un usuario administrativo autorizado registra un producto con su información comercial (nombre, categoría, descripción, imagen ilustrativa, precio o información para determinarlo, disponibilidad) y puede consultarlo y modificarlo.

**Why this priority**: es el núcleo del catálogo y prerrequisito de todas las funcionalidades consumidoras.

**Independent Test**: se puede probar registrando un producto en una categoría existente y verificando su consulta y modificación.

**Acceptance Scenarios**:

1. **Given** una categoría existente, **When** el usuario registra un producto asociado a ella con su información comercial, **Then** el producto aparece en el catálogo con esa información.
2. **Given** un producto existente, **When** el usuario modifica su información comercial, **Then** la consulta muestra los valores actualizados.
3. **Given** un producto sin imagen, **When** el usuario lo consulta, **Then** el comportamiento respeta lo que se defina en la aclaración de imagen (ver Cuestiones pendientes).

---

### User Story 3 - Definir variantes flexibles de un producto (Priority: P1)

Un usuario administrativo autorizado define, para un producto, las características de variación que necesita y sus opciones comercializables, sin que el sistema imponga una estructura de variación igual para todos los productos.

**Why this priority**: el catálogo actual mezcla tamaños, modalidades, variedades, presentaciones y sabores; la flexibilidad es un requisito explícito (Requerimientos §10 y §17).

**Independent Test**: se prueba representando los escenarios 1 a 5 de la sección "Escenarios funcionales de flexibilidad" sin crear tipos ni campos específicos por producto.

**Acceptance Scenarios**:

1. **Given** el producto "papas fritas", **When** el usuario define las opciones chica y grande, **Then** el producto muestra ambas opciones comercializables y no se requirió ningún tipo de dato específico de papas fritas.
2. **Given** el producto "hamburguesa" (o "milanesa"), **When** el usuario define las opciones simple y completa, **Then** ambas opciones quedan consultables de forma inequívoca.
3. **Given** el producto "pizza", **When** el usuario define varias variedades, **Then** todas las variedades son opciones del mismo producto o se representan sin una estructura de producto distinta por variedad.
4. **Given** una bebida, **When** el usuario define presentaciones como 500 ml, 750 ml, 1 litro y 1,5 litros, **Then** las presentaciones quedan registradas y el usuario puede definir una presentación no prevista (por ejemplo otra capacidad) sin cambiar el sistema.
5. **Given** un agua saborizada, **When** el usuario define opciones distinguidas por presentación y por sabor a la vez, **Then** cada combinación es una opción comercializable distinguible y consultable, sin reglas de excepción para este producto.
6. **Given** un producto con varias opciones, **When** el usuario consulta el producto, **Then** ve de forma inequívoca todas sus opciones comercializables.

---

### User Story 4 - Modificar el catálogo existente (Priority: P2)

Un usuario administrativo autorizado modifica la información comercial de categorías, productos y opciones de venta ya registrados, respetando las reglas definidas en esta especificación.

**Why this priority**: el catálogo cambia con la operación del bar, pero requiere que primero exista la gestión básica.

**Independent Test**: se prueba modificando datos de una categoría, un producto y una opción y consultando el resultado.

**Acceptance Scenarios**:

1. **Given** un producto con opciones, **When** el usuario modifica la información de una opción, **Then** solo esa opción refleja el cambio y el producto conserva sus otras opciones.
2. **Given** una categoría con productos, **When** el usuario modifica su información, **Then** los productos asociados siguen asociados a ella.
3. **Given** un producto existente, **When** el usuario modifica su precio, **Then** las funcionalidades consumidoras obtienen el precio vigente sin ambigüedad (sin historial de precios; ver Precio).

---

### User Story 5 - Extensibilidad del catálogo (Priority: P3)

Un usuario administrativo autorizado puede incorporar productos y categorías de un tipo nuevo (por ejemplo, tragos en el futuro) sin que cambie el concepto general de categoría, producto y variante.

**Why this priority**: garantiza que el catálogo no quedó modelado en torno a los productos actuales (Requerimientos §9.3).

**Independent Test**: se prueba definiendo una categoría y productos de ejemplo distintos a los actuales usando solo los conceptos existentes. Los tragos NO se incorporan al alcance actual.

**Acceptance Scenarios**:

1. **Given** el catálogo vigente, **When** el usuario define una categoría nueva con productos cuyas características de variación no se usan hoy, **Then** los conceptos de categoría, producto y variante no requieren modificarse.

## Escenarios funcionales de flexibilidad

Estos escenarios verifican la flexibilidad del modelo; NO son tipos de producto ni estructuras del catálogo.

1. **Tamaño**: papas fritas con opciones chica y grande.
2. **Modalidad**: hamburguesa o milanesa con opciones simple y completa.
3. **Variedades**: pizzas de distintas variedades.
4. **Presentaciones**: una bebida con 500 ml, 750 ml, 1 litro y 1,5 litros, sin lista global cerrada.
5. **Varias características**: agua saborizada distinguida por presentación y sabor.
6. **Futuro**: productos de una categoría nueva (tragos) sin modificar los conceptos generales.
7. **Modificación**: cambio de información comercial de categorías, productos y opciones.

### Edge Cases

Cada caso indica lo que esta especificación establece o la aclaración pendiente. No se inventan políticas sin respaldo en los requerimientos.

- **Producto sin variantes**: es válido; tiene su propio precio y es seleccionable sin ambigüedad (FR-008).
- **Producto con una sola variante**: el producto DEBE seguir siendo consultable y seleccionable sin ambigüedad.
- **Producto con múltiples características de variación**: DEBE ser posible (aguas saborizadas).
- **Dos variantes funcionalmente indistinguibles** de un mismo producto: el sistema NO DEBE permitir que una opción comercializable resulte ambigua respecto de otra del mismo producto (FR-011); los detalles de unicidad están pendientes (Cuestión 8).
- **Categoría sin productos**: DEBE ser válida y consultable.
- **Cambio de categoría de un producto / modificación de una categoría**: la modificación de la categoría NO DEBE romper la asociación de sus productos.
- **Modificación de una variante existente**: pendiente en lo relativo a referencias existentes (Cuestión 7, parte de modificación).
- **Producto sin imagen**: pendiente (Cuestión 9).
- **Disponibilidad no determinada todavía**: pendiente (Cuestión 4).
- **Modificaciones de precio**: no se define historial; el precio consultado es el vigente (ver Precio).
- **Nombres repetidos** de categorías, productos u opciones: pendiente (Cuestión 8).
- **Retiro o baja** de categorías o productos que otras funcionalidades puedan referenciar: la baja es reversible, no hay eliminación definitiva y las referencias existentes siguen válidas sin nuevos usos (FR-018).

## Requirements *(mandatory)*

### Functional Requirements

**Categorías**

- **FR-001**: El usuario administrativo autorizado DEBE poder crear categorías.
- **FR-002**: El usuario administrativo autorizado DEBE poder consultar las categorías existentes.
- **FR-003**: El usuario administrativo autorizado DEBE poder modificar la información de una categoría.
- **FR-004**: Todo producto DEBE pertenecer a exactamente una categoría.

**Productos**

- **FR-005**: El usuario administrativo autorizado DEBE poder registrar productos con su información comercial: nombre, categoría, descripción, imagen ilustrativa, información de precio y disponibilidad para otras funcionalidades.
- **FR-006**: El usuario administrativo autorizado DEBE poder consultar y modificar la información comercial de un producto.
- **FR-007**: El catálogo NO DEBE exigir que todos los productos compartan los mismos campos adicionales ni la misma estructura de variación.

**Variantes**

- **FR-008**: Un producto PUEDE no tener variantes y venderse como una única opción con su propio precio; cuando corresponda, DEBE poder representar distintas opciones comercializables (variantes).
- **FR-009**: Cada producto DEBE poder definir sus propias características de variación; el catálogo NO DEBE limitarlas a un conjunto global cerrado ni codificar valores como tamaños, presentaciones, sabores o modalidades.
- **FR-010**: Una opción comercializable DEBE poder identificarse por más de una característica de variación a la vez (por ejemplo presentación y sabor).
- **FR-011**: El usuario administrativo autorizado DEBE poder consultar de forma inequívoca qué opciones comercializables tiene un producto, y dos opciones del mismo producto NO DEBEN resultar funcionalmente indistinguibles.
- **FR-012**: El usuario administrativo autorizado DEBE poder modificar la información comercial de una opción comercializable existente.

**Precio**

- **FR-013**: El precio DEBE pertenecer a cada opción comercializable: cada variante tiene su propio precio, determinable y no ambiguo cuando lo utilicen las funcionalidades consumidoras. NO existe precio base con ajustes ni precio compartido obligatorio entre variantes. Un producto sin variantes tiene su propio precio.
- **FR-014**: El usuario administrativo autorizado DEBE poder modificar el precio; el sistema NO DEBE registrar historial de precios en esta feature.

**Disponibilidad e imagen**

- **FR-015**: El catálogo DEBE poder expresar la información de disponibilidad que necesiten sus consumidores. La fuente y las reglas que la determinan NO se definen en esta feature.
- **FR-016**: Un producto DEBE poder tener información de imagen ilustrativa para su presentación pública posterior.

**Extensibilidad**

- **FR-017**: Agregar nuevos productos y nuevas formas razonables de variación NO DEBE exigir redefinir funcionalmente el catálogo; DEBE ser posible incorporar en el futuro una categoría de tragos sin cambiar los conceptos de categoría, producto y variante.

**Retiro del catálogo y consumo**

- **FR-018**: El usuario administrativo autorizado DEBE poder dar de baja (desactivar) categorías y productos del catálogo de forma reversible, es decir, poder reactivarlos. El catálogo NO DEBE permitir su eliminación definitiva. Un elemento dado de baja DEBE conservar su información y las referencias existentes hacia él DEBEN seguir siendo válidas, pero NO DEBE poder seleccionarse para nuevos usos. Su efecto sobre la disponibilidad para venta y la visibilidad pública se define en la cuestión 5.
- **FR-019**: Las funcionalidades consumidoras (POS, recetas, inventario, catálogo público) DEBEN poder seleccionar inequívocamente un producto o una opción comercializable del catálogo.

### Key Entities *(include if feature involves data)*

- **Categoría**: agrupación de productos, con información propia modificable.
- **Producto**: bien comercializado, con información comercial (nombre, descripción, imagen ilustrativa, información de precio, disponibilidad) y relación con su categoría.
- **Característica de variación**: eje de variación definido por producto (por ejemplo presentación o sabor).
- **Opción comercializable (variante)**: alternativa de venta de un producto, identificada por valores de sus características de variación, con precio determinable.

### Requisitos no funcionales

- Las reglas constitucionales que aplican a esta feature (Principios I, II, VIII y IX) rigen sin repetirse aquí; no se definen requisitos no funcionales adicionales.
- La feature no requiere ni impone métricas numéricas: los requerimientos no las definen.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Los siete escenarios funcionales de la sección "Escenarios funcionales de flexibilidad" se pueden representar íntegramente en el catálogo sin tipos de producto ni campos específicos por producto.
- **SC-002**: Todos los productos actuales de Requerimientos §9 (papas fritas, milanesas, hamburguesas, pizzas, gaseosas, agua mineral, cerveza, agua saborizada) se pueden representar con las opciones mencionadas.
- **SC-003**: Para cualquier producto, el usuario administrativo puede consultar sus opciones comercializables sin ambigüedad.
- **SC-004**: Ninguna funcionalidad consumidora necesita interpretar el precio de una opción de forma ambigua.
- **SC-005**: Incorporar una categoría y productos de un tipo nuevo no requiere modificar los conceptos de categoría, producto ni variante.

## Assumptions

- El actor es el "usuario administrativo autorizado"; roles, permisos y autenticación pertenecen a la especificación de usuarios y roles.
- No se define historial de precios porque los requerimientos no lo exigen.
- La representación técnica del catálogo, la imagen y la disponibilidad se define en `/speckit.plan` y no se decide aquí.
- Los ejemplos de productos y presentaciones son escenarios de verificación, no restricciones del modelo (Requerimientos §17).
- Depende de las especificaciones posteriores de POS, recetas, inventario y catálogo público como consumidoras del catálogo.

## Cuestiones pendientes para `/speckit.clarify`

Las cuestiones 1, 2, 3 y 6 ya están resueltas (ver Clarifications). Ya no quedan marcadores `[NEEDS CLARIFICATION]`. Las demás se derivan de los casos límite y NO se resuelven por suposición.

1. ~~¿Un producto puede pertenecer a una sola categoría o a varias?~~ Resuelta: exactamente una (FR-004).
2. ~~¿Cómo se determina el precio cuando existen variantes?~~ Resuelta: cada variante tiene su propio precio (FR-013).
3. ~~¿Puede existir un producto sin variantes?~~ Resuelta: sí, con su propio precio (FR-008).
4. ¿Qué significa funcionalmente que un producto esté "disponible"?
5. ¿Existe diferencia entre producto activo, disponible para venta y visible públicamente?
6. ~~¿Cómo se retira un producto o categoría?~~ Resuelta: solo baja/desactivación reversible, sin eliminación definitiva (FR-018).
7. ~~¿Qué ocurre con las referencias existentes al retirar un producto o variante?~~ Resuelta para la baja: se conservan y siguen válidas, sin nuevos usos (FR-018). Sigue pendiente el efecto de modificar una variante sobre referencias existentes.
8. ¿Qué reglas de unicidad existen para categorías, productos, características y opciones de variantes?
9. ¿La imagen es obligatoria o puede faltar?
10. ¿Las variantes pueden cambiar individualmente información comercial además del precio, y cuál?

## Fuera de alcance

Pedidos, POS, mesas y barra, estados de pedidos, carrito público, interfaz del catálogo público, QR, caja, cobros, ventas, tickets, ingredientes, recetas, movimientos y control de stock, gestión de usuarios, roles y permisos, dashboard, KPIs, reportes, múltiples sucursales, clientes, empleados, cocina, chefs y delivery. Estas capacidades no se introducen indirectamente mediante el modelo del catálogo.
