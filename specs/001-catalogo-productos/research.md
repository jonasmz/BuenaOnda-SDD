# Research: Gestión del catálogo de productos

Decisiones técnicas de Phase 0. No queda ningún `NEEDS CLARIFICATION` del Technical Context; las cuestiones funcionales abiertas de la spec se listan al final y NO se resuelven aquí.

## R-1. Estructura hexagonal de la solución backend

- **Decision**: cuatro proyectos: `Domain` (sin referencias a otros proyectos ni paquetes externos), `Application` (referencia a Domain; define los puertos de salida), `Infrastructure` (referencia a Application; implementa persistencia con EF Core y Npgsql) y `Api` (adaptador de entrada; referencia a Application; componer dependencias en el arranque).
- **Rationale**: cumple el Principio III y hace verificable por referencias de proyecto que dominio y casos de uso no ven EF Core. El proyecto `Api` referencia a `Infrastructure` solo para el registro de dependencias (composition root).
- **Alternatives considered**: un único proyecto con carpetas (no hace cumplir la dirección de dependencias por compilación); proyectos por módulo funcional (innecesario para una sola feature).

## R-2. Agregado Producto con características y opciones

- **Decision**: `Producto` es la raíz de un agregado que contiene sus características de variación y sus opciones comercializables. `Categoría` es un agregado separado.
- **Rationale**: la unicidad de las opciones dentro de un producto (FR-011) es un invariante que solo se puede garantizar de forma consistente si el producto controla el conjunto completo de opciones.
- **Alternatives considered**: opciones como agregados independientes (no pueden garantizar por sí solas la distinción entre opciones del mismo producto).

## R-3. Toda opción vendible es una opción comercializable con precio propio

- **Decision**: un producto sin características de variación tiene exactamente una opción con valores vacíos y su propio precio; un producto con características tiene una opción por combinación definida. El precio se ubica siempre en la opción.
- **Rationale**: FR-008 y FR-013 (precio por opción, producto sin variantes con precio propio) y FR-019 (selección inequívoca) se satisfacen con un único concepto seleccionable; POS, recetas e inventario referencian siempre un identificador de opción.
- **Alternatives considered**: precio en el producto cuando no hay variantes (obliga a los consumidores a manejar dos formas de referencia).

## R-4. Características y valores como datos

- **Decision**: una característica es un par nombre/valores definido por producto; el valor de una opción para una característica es texto libre normalizado para comparar. No hay enumeraciones ni columnas por característica.
- **Rationale**: FR-007, FR-009, FR-010 y FR-017. Unidades y capacidades (por ejemplo "750 ml") son texto del catálogo; el sistema no interpreta unidades de medida, que pertenecen a especificaciones posteriores.
- **Alternatives considered**: catálogo global de características o de valores (impone una lista global cerrada, contraria a FR-009).

## R-5. Distinción entre opciones (FR-011)

- **Decision**: dos opciones de un mismo producto no pueden tener el mismo conjunto de pares característica/valor, comparados sin distinguir mayúsculas ni espacios de borde. Los nombres de característica son únicos dentro del producto por la misma comparación. Se valida en el dominio y se refuerza con una restricción de unicidad en la base de datos.
- **Rationale**: es el mínimo necesario para que la opción sea inequívoca sin decidir la unicidad de nombres de categorías o productos (Cuestión 8 abierta).
- **Alternatives considered**: no imponer nada (contradice FR-011).

## R-6. Identificadores estables para los consumidores

- **Decision**: cada categoría, producto y opción tiene un identificador único inmutable, y editar su información nunca lo cambia.
- **Rationale**: FR-019 y FR-018 (las referencias existentes siguen válidas). Los consumidores futuros referencian opciones por identificador.
- **Alternatives considered**: usar el nombre como clave (frágil ante ediciones).

## R-7. Baja reversible

- **Decision**: categorías y productos tienen un estado activo/inactivo. No hay operación de eliminación definitiva. Un elemento inactivo conserva su información; no puede elegirse para nuevos usos dentro del catálogo: no se puede asignar un producto a una categoría inactiva ni crear opciones o editar precios de un producto inactivo hasta reactivarlo (se permite reactivar y consultar).
- **Rationale**: FR-018 y las decisiones de clarificación. La exposición a otros consumidores (POS, público) se define en sus features.
- **Alternatives considered**: eliminación física con verificación de referencias (descartada por la clarificación).

## R-8. Pruebas, sin ampliar el stack obligatorio

- **Decision**: xUnit en backend (pruebas de dominio, de casos de uso con dobles de los puertos, e integración de la API contra una base dedicada en `psql-17`) y el ejecutor por defecto del proyecto Angular en frontend.
- **Rationale**: dependencias complementarias sin decisiones arquitectónicas contrarias a la constitución. Reutilizar `psql-17` evita agregar herramientas de contenedores que la constitución no requiere.
- **Alternatives considered**: bases en memoria (no reproducen restricciones reales de PostgreSQL); librerías de contenedores de prueba (dependencia adicional innecesaria).

## R-9. Imagen ilustrativa

- **Decision**: el producto guarda una referencia de imagen opcional en forma de texto (URL). Esta feature no sube, almacena ni transforma archivos.
- **Rationale**: FR-016 pide información de imagen para presentación posterior, no un mecanismo de carga. Es la decisión técnica más simple compatible con la spec. La obligatoriedad (Cuestión 9) sigue abierta; se permite ausencia porque no hay regla que la exija.
- **Alternatives considered**: carga de archivos con almacenamiento propio (amplía el alcance y decide almacenamiento sin requisito).

## R-10. Disponibilidad (FR-015)

- **Decision**: esta feature NO persiste ni calcula disponibilidad. El único estado de vigencia del catálogo es activo/inactivo (R-7). El contrato de lectura se puede extender de forma compatible cuando se definan las Cuestiones 4 y 5.
- **Rationale**: la spec prohíbe decidir la fuente y la semántica de la disponibilidad. Agregar un indicador manual la decidiría por suposición.
- **Riesgo**: FR-015 queda satisfecho solo en forma de capacidad de extensión. Se recomienda resolver las Cuestiones 4 y 5 en `/speckit-clarify` antes de `/speckit-tasks` si se quiere que la feature entregue disponibilidad.

## R-11. Contrato HTTP administrativo

- **Decision**: API REST con JSON, controladores en el adaptador `Api`, todas las rutas bajo `/api/admin/catalog`, errores en formato ProblemDetails. Detalle en [contracts/catalog-admin-api.md](./contracts/catalog-admin-api.md).
- **Rationale**: prefijo administrativo único para aplicar autenticación y autorización de forma centralizada cuando exista la feature de usuarios y roles (Principio IV).
- **Alternatives considered**: reutilizar rutas sin prefijo (mezcla lo administrativo con una futura superficie pública).

## R-12. Frontend administrativo

- **Decision**: aplicación Angular 22 en `frontend/admin` con un módulo funcional `catalog` (páginas de categorías y productos, formulario de opciones, servicio HTTP y modelos). Interfaz con CoreUI for Angular Free sobre Bootstrap 5 y Minty.
- **Rationale**: es la organización mínima que cumple el stack visual; no define la arquitectura de otras features.
- **Alternatives considered**: monorepo con librerías compartidas entre frontends (prematuro; Principio V exige independencia).

## Cuestiones funcionales que siguen abiertas (no resueltas aquí)

- Cuestión 4 y 5: significado de "disponible" y diferencia entre activo, vendible y visible (afecta R-10).
- Cuestión 7 (parte de modificación): efecto de modificar una variante sobre referencias existentes. Por R-6 el identificador no cambia.
- Cuestión 8: unicidad de nombres de categorías y productos. Solo se impone R-5.
- Cuestión 9: obligatoriedad de la imagen (R-9 la trata como opcional).
- Cuestión 10: qué información, además del precio, puede variar por opción. Hoy la opción solo tiene valores de variación, precio y estado de vigencia implícito en el producto.
- Baja de opciones individuales: la spec no define retirar una opción; se propone tratarlo en clarify.
