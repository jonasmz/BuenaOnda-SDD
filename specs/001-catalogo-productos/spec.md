# Especificación funcional: Gestión del catálogo de productos

**Feature Branch**: `001-catalogo-productos`

**Created**: 2026-09-21

**Status**: Draft (revisada el 2026-09-21: catálogo plano, sin variantes; constitución v2.0.0)

**Input**: Feature "001 — Gestión del catálogo de productos": permitir que un usuario administrativo autorizado gestione la definición comercial del catálogo (categorías, productos, información comercial y precios). Cada tamaño, presentación, sabor o variedad que se vende por separado es un producto distinto; el catálogo no tiene variantes.

## Propósito y conceptos

Esta especificación es la fuente funcional de verdad de los conceptos **categoría** y **producto**. Otras funcionalidades (POS, recetas, inventario, catálogo público) los consumirán posteriormente; aquí solo se define el catálogo comercial.

Conceptos funcionales usados en este documento:

- **Categoría**: agrupación con la que se organizan los productos.
- **Producto**: bien que el bar vende como una unidad, con su información comercial, su precio y su disponibilidad. Si algo se vende en distintas presentaciones (por ejemplo papas fritas chicas y grandes), cada presentación es un producto distinto con su propio nombre y precio. No existen variantes, opciones ni características de variación.
- **Baja**: retiro reversible de una categoría o producto (también "desactivar"); "reactivar" la revierte. No existe eliminación definitiva.
- **Usuario administrativo autorizado**: actor genérico. Los roles, permisos y la autenticación quedan fuera de esta especificación.

## Clarifications

### Session 2026-09-21

- Q: ¿Un producto puede pertenecer a una sola categoría o a varias categorías a la vez? → A: Exactamente una categoría por producto.
- Q: ¿Cómo se retira un producto o categoría del catálogo? → A: Solo baja/desactivación reversible; no hay eliminación definitiva.
- Q: Al dar de baja un producto, ¿qué pasa con su uso en el resto del sistema? → A: Se conserva y las referencias existentes siguen válidas; no se puede seleccionar para nuevos usos.
- Q: ¿Qué significa que un producto esté "disponible" y a qué nivel se marca? → A: Marca manual del usuario administrativo, por producto; no se deriva del inventario en esta feature.
- Q: ¿Un producto o categoría dado de baja queda automáticamente no disponible y no visible al público? → A: Sí; la baja implica no disponible y no visible, y es un dato aparte de la marca manual de disponibilidad del producto.
- Q: ¿Se permiten nombres repetidos de categorías o de productos? → A: No; nombre único entre categorías y nombre de producto único dentro de su categoría.
- Q: ¿Se puede registrar un producto sin imagen? → A: Sí, la imagen es opcional al registrar, pero es obligatoria para ofrecerlo públicamente (sin imagen no es visible al público).
- Q: ¿Se admiten variantes de producto (tamaños, presentaciones, sabores, modalidades)? → A: No. Decisión del responsable del proyecto: se eliminan por completo por ser complejidad innecesaria; cada presentación o variedad es un producto distinto (constitución v2.0.0, Principio VIII). No se agrupan ni se relacionan entre sí.
- Q: ¿Cómo se representa la marca de una gaseosa y sus presentaciones? → A: Un producto por cada marca y presentación (por ejemplo "Coca-Cola 500 ml", "Coca-Cola 1,5 litros").

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Administrar categorías (Priority: P1)

Un usuario administrativo autorizado registra, consulta y modifica las categorías con las que se organiza la oferta del bar.

**Why this priority**: sin categorías no puede organizarse el catálogo ni, más adelante, filtrarse por ellas.

**Independent Test**: se puede probar creando, consultando y modificando categorías sin que existan productos.

**Acceptance Scenarios**:

1. **Given** un catálogo sin categorías, **When** el usuario registra una categoría con su información, **Then** la categoría aparece en la consulta de categorías.
2. **Given** una categoría existente, **When** el usuario modifica su información, **Then** la consulta muestra la información actualizada.
3. **Given** una categoría sin productos, **When** el usuario la consulta, **Then** se muestra sin productos asociados y sin error.
4. **Given** una categoría existente llamada "Bebidas", **When** el usuario crea otra categoría con el mismo nombre sin distinguir mayúsculas ni espacios de borde, **Then** la creación se rechaza y el catálogo no cambia (FR-015).

---

### User Story 2 - Registrar y consultar productos (Priority: P1)

Un usuario administrativo autorizado registra un producto con su información comercial (nombre, categoría, descripción, imagen ilustrativa, precio y disponibilidad) y puede consultarlo y modificarlo.

**Why this priority**: es el núcleo del catálogo y prerrequisito de todas las funcionalidades consumidoras.

**Independent Test**: se puede probar registrando un producto en una categoría existente y verificando su consulta y modificación.

**Acceptance Scenarios**:

1. **Given** una categoría existente, **When** el usuario registra un producto asociado a ella con su información comercial, precio y disponibilidad, **Then** el producto aparece en el catálogo con esa información.
2. **Given** un producto existente, **When** el usuario modifica su información comercial o su precio, **Then** la consulta muestra los valores actualizados y las funcionalidades consumidoras obtienen el precio vigente sin ambigüedad (sin historial de precios).
3. **Given** un producto sin imagen, **When** el usuario lo registra o consulta, **Then** el producto se acepta y se identifica como sin imagen, y no es visible al público hasta que se le asigne una (FR-011).
4. **Given** un producto existente en una categoría, **When** el usuario registra otro producto con el mismo nombre en esa misma categoría, **Then** se rechaza; **When** lo registra en otra categoría, **Then** se acepta (FR-015).
5. **Given** un producto activo, de categoría activa y con imagen, **When** el usuario consulta su visibilidad, **Then** es visible al público; **When** el producto no tiene imagen, está dado de baja o su categoría está dada de baja, **Then** no es visible al público (FR-011, FR-013).
6. **Given** un producto "Papas fritas chicas" y otro "Papas fritas grandes", **When** el usuario los consulta, **Then** cada uno figura con su propio precio y disponibilidad, sin relación entre ellos.

---

### User Story 3 - Modificar, dar de baja y reactivar el catálogo (Priority: P2)

Un usuario administrativo autorizado modifica la información de categorías y productos ya registrados, marca la disponibilidad de los productos y da de baja y reactiva categorías y productos.

**Why this priority**: el catálogo cambia con la operación del bar, pero requiere que primero exista la gestión básica.

**Independent Test**: se prueba modificando una categoría y un producto, marcando disponibilidad y dando de baja y reactivando, y consultando el resultado.

**Acceptance Scenarios**:

1. **Given** una categoría con productos, **When** el usuario modifica su información, **Then** los productos asociados siguen asociados a ella.
2. **Given** un producto disponible, **When** el usuario lo marca como no disponible, **Then** la consulta lo muestra no disponible; **When** lo marca disponible, **Then** vuelve a mostrarse disponible (FR-010).
3. **Given** un producto marcado como no disponible, **When** el usuario lo da de baja y luego lo reactiva, **Then** durante la baja no está disponible ni es visible al público, y tras la reactivación su marca de disponibilidad se conserva tal como estaba (FR-010, FR-013).
4. **Given** una categoría con productos, **When** el usuario la da de baja, **Then** sus productos dejan de estar disponibles y visibles, conservan su información y no se pueden asignar nuevos productos a esa categoría; **When** la reactiva, **Then** vuelven a su estado anterior (FR-013).
5. **Given** un producto o categoría dado de baja, **When** el usuario intenta modificarlo, **Then** se rechaza hasta que se reactive; la consulta y la reactivación siempre se permiten (FR-013).

---

### User Story 4 - Extensibilidad del catálogo (Priority: P3)

Un usuario administrativo autorizado puede incorporar productos y categorías de un tipo nuevo (por ejemplo, tragos en el futuro) sin que cambien los conceptos generales de categoría y producto.

**Why this priority**: garantiza que el catálogo no quedó modelado en torno a los productos actuales (Requerimientos §9.3).

**Independent Test**: se prueba definiendo una categoría y productos de ejemplo distintos a los actuales usando solo los conceptos existentes. Los tragos NO se incorporan al alcance actual.

**Acceptance Scenarios**:

1. **Given** el catálogo vigente, **When** el usuario define una categoría nueva con productos distintos a los actuales, **Then** los conceptos de categoría y producto no requieren modificarse.

## Escenarios funcionales de catálogo plano

Estos escenarios verifican que el modelo plano alcanza; NO son tipos de producto ni estructuras del catálogo.

1. **Tamaño**: "Papas fritas chicas" y "Papas fritas grandes", dos productos con su propio precio.
2. **Modalidad**: "Hamburguesa simple" y "Hamburguesa completa" (o "Milanesa simple" y "Milanesa completa").
3. **Variedades**: cada pizza es un producto ("Pizza muzzarella", "Pizza fugazzeta").
4. **Presentaciones**: una bebida en 500 ml, 750 ml, 1 litro y 1,5 litros son cuatro productos ("Coca-Cola 500 ml", "Coca-Cola 1,5 litros"); una presentación nueva es un producto nuevo.
5. **Combinaciones**: "Agua saborizada pomelo 500 ml" y "Agua saborizada manzana 1,5 litros" son productos independientes.
6. **Futuro**: productos de una categoría nueva (tragos) sin modificar los conceptos generales.
7. **Modificación**: cambio de información, precio y disponibilidad de categorías y productos.

### Edge Cases

- **Categoría sin productos**: DEBE ser válida y consultable.
- **Modificación de una categoría**: NO DEBE romper la asociación de sus productos.
- **Cambio de categoría de un producto**: el destino DEBE estar activo y no tener otro producto con el mismo nombre (FR-015).
- **Producto sin imagen**: se puede registrar y consultar en la administración, pero no es visible al público hasta que tenga imagen (FR-011).
- **Disponibilidad**: se marca manualmente por producto (FR-010); la baja implica no disponible y no visible al público (FR-013).
- **Modificaciones de precio**: no se define historial; el precio consultado es el vigente.
- **Nombres repetidos**: categorías y productos se rigen por FR-015; queda pendiente si se comparan también acentos.
- **Baja** de categorías o productos que otras funcionalidades puedan referenciar: es reversible, no hay eliminación definitiva y las referencias existentes siguen válidas sin nuevos usos (FR-013).
- **Modificar un producto referenciado** (por ejemplo su precio): pendiente en lo relativo a referencias existentes; el identificador no cambia.

## Requirements *(mandatory)*

### Functional Requirements

**Categorías**

- **FR-001**: El usuario administrativo autorizado DEBE poder crear categorías.
- **FR-002**: El usuario administrativo autorizado DEBE poder consultar las categorías existentes.
- **FR-003**: El usuario administrativo autorizado DEBE poder modificar la información de una categoría.
- **FR-004**: Todo producto DEBE pertenecer a exactamente una categoría.

**Productos**

- **FR-005**: El usuario administrativo autorizado DEBE poder registrar productos con su información comercial: nombre, categoría, descripción, imagen ilustrativa, precio y disponibilidad.
- **FR-006**: El usuario administrativo autorizado DEBE poder consultar y modificar la información comercial de un producto.
- **FR-007**: El catálogo es plano: NO DEBE incluir variantes, opciones ni características de variación. Cada presentación o variedad que se vende por separado es un producto distinto, y el catálogo NO DEBE exigir que todos los productos compartan campos adicionales.

**Precio**

- **FR-008**: Cada producto DEBE tener un único precio, determinable y no ambiguo cuando lo utilicen las funcionalidades consumidoras.
- **FR-009**: El usuario administrativo autorizado DEBE poder modificar el precio; el sistema NO DEBE registrar historial de precios en esta feature.

**Disponibilidad e imagen**

- **FR-010**: El usuario administrativo autorizado DEBE poder marcar manualmente cada producto como disponible o no disponible, y el catálogo DEBE exponer ese dato a sus consumidores. Esta feature NO deriva la disponibilidad del inventario; cualquier vinculación futura con el inventario se define en su propia especificación.
- **FR-011**: Un producto DEBE poder tener información de imagen ilustrativa para su presentación pública posterior. La imagen es opcional al registrar el producto, pero un producto sin imagen NO DEBE ser visible al público; el catálogo DEBE permitir saber si un producto tiene imagen.

**Extensibilidad**

- **FR-012**: Agregar nuevos productos NO DEBE exigir redefinir funcionalmente el catálogo; DEBE ser posible incorporar en el futuro una categoría de tragos sin cambiar los conceptos de categoría y producto.

**Retiro del catálogo y consumo**

- **FR-013**: El usuario administrativo autorizado DEBE poder dar de baja (desactivar) categorías y productos de forma reversible, es decir, poder reactivarlos. El catálogo NO DEBE permitir su eliminación definitiva. Un elemento dado de baja DEBE conservar su información y las referencias existentes hacia él DEBEN seguir siendo válidas, pero NO DEBE poder seleccionarse para nuevos usos ni modificarse hasta reactivarlo. La baja de un producto o categoría implica que el producto no está disponible ni es visible al público; la baja es un dato distinto de la marca manual de disponibilidad del producto (FR-010), que no se modifica por la baja ni por la reactivación.
- **FR-014**: Las funcionalidades consumidoras (POS, recetas, inventario, catálogo público) DEBEN poder seleccionar inequívocamente un producto del catálogo.

**Unicidad**

- **FR-015**: El nombre de una categoría DEBE ser único entre categorías, y el nombre de un producto DEBE ser único dentro de su categoría, sin distinguir mayúsculas ni espacios de borde.

### Key Entities *(include if feature involves data)*

- **Categoría**: agrupación de productos, con información propia modificable.
- **Producto**: bien comercializado, con información comercial (nombre, descripción, imagen ilustrativa), precio, marca manual de disponibilidad, relación con su categoría y estado de baja reversible.

### Requisitos no funcionales

- Las reglas constitucionales que aplican a esta feature (Principios I, II, VIII y IX) rigen sin repetirse aquí; no se definen requisitos no funcionales adicionales.
- La feature no requiere ni impone métricas numéricas: los requerimientos no las definen.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Los siete escenarios funcionales de la sección "Escenarios funcionales de catálogo plano" se pueden representar íntegramente sin tipos de producto ni campos específicos por producto.
- **SC-002**: Todos los productos actuales de Requerimientos §9 (papas fritas, milanesas, hamburguesas, pizzas, gaseosas, agua mineral, cerveza, agua saborizada) se pueden representar como productos planos, uno por cada presentación o variedad que se vende.
- **SC-003**: Para cualquier producto, el usuario administrativo puede consultar su precio y disponibilidad sin ambigüedad.
- **SC-004**: Ninguna funcionalidad consumidora necesita interpretar el precio de un producto de forma ambigua.
- **SC-005**: Incorporar una categoría y productos de un tipo nuevo no requiere modificar los conceptos de categoría ni producto.

## Assumptions

- El actor es el "usuario administrativo autorizado"; roles, permisos y autenticación pertenecen a la especificación de usuarios y roles.
- No se define historial de precios porque los requerimientos no lo exigen.
- La representación técnica del catálogo, la imagen y la disponibilidad se define en `/speckit.plan` y no se decide aquí.
- Los ejemplos de productos son escenarios de verificación, no restricciones del modelo (Requerimientos §17).
- El precio es un valor no negativo en una única moneda y el precio 0 es válido. La imagen se guarda como referencia (URL) en forma de texto.
- Los requerimientos funcionales (`system_requirements`) mencionan variantes; deben actualizarse para reflejar el catálogo plano (constitución v2.0.0, Principio IX).
- Depende de las especificaciones posteriores de POS, recetas, inventario y catálogo público como consumidoras del catálogo.

## Cuestiones de clarificación

Todas las cuestiones funcionales están resueltas. Solo quedan abiertas dos precisiones menores, sin marcadores `[NEEDS CLARIFICATION]`:

1. Efecto de modificar un producto (por ejemplo su precio) sobre referencias existentes de funcionalidades consumidoras: el identificador no cambia.
2. Comparación de acentos en los nombres: hoy son significativos.

## Fuera de alcance

Variantes, opciones o características de variación de productos; pedidos, POS, mesas y barra, estados de pedidos, carrito público, interfaz del catálogo público, QR, caja, cobros, ventas, tickets, ingredientes, recetas, movimientos y control de stock, gestión de usuarios, roles y permisos, dashboard, KPIs, reportes, múltiples sucursales, clientes, empleados, cocina, chefs y delivery. Estas capacidades no se introducen indirectamente mediante el modelo del catálogo.
