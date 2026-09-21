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

## R-5. Unicidad y distinción (FR-020, FR-011)

- **Decision**: (a) el nombre de una categoría es único entre categorías; (b) el nombre de un producto es único dentro de su categoría, incluso al moverlo a otra; (c) dos opciones de un mismo producto no pueden tener el mismo conjunto de pares característica/valor; (d) los nombres de característica son únicos dentro del producto. Todas las comparaciones ignoran mayúsculas y espacios de borde (los acentos siguen siendo significativos). Se validan en el dominio y se refuerzan con restricciones de unicidad en la base de datos sobre la forma normalizada.
- **Rationale**: FR-020 y FR-011 definen las reglas; el refuerzo en base de datos evita duplicados por concurrencia.
- **Alternatives considered**: solo validar en la aplicación (permite duplicados bajo concurrencia).

## R-5b. Valores reservados

- **Decision**: los valores de variación de una opción dada de baja siguen contando para la distinción de opciones (R-5) mientras la opción exista. Para volver a usar esa combinación se reactiva la opción o se la elimina si no fue referenciada.
- **Rationale**: FR-021 y FR-011; evita dos opciones indistinguibles en cualquier momento.
- **Alternatives considered**: ignorar las opciones inactivas al comprobar la unicidad (permite ambigüedad al reactivar).

## R-6. Identificadores estables para los consumidores

- **Decision**: cada categoría, producto y opción tiene un identificador único inmutable, y editar su información nunca lo cambia.
- **Rationale**: FR-019 y FR-018 (las referencias existentes siguen válidas). Los consumidores futuros referencian opciones por identificador.
- **Alternatives considered**: usar el nombre como clave (frágil ante ediciones).

## R-7. Baja reversible

- **Decision**: categorías y productos tienen un estado activo/inactivo. No hay operación de eliminación definitiva. Un elemento inactivo conserva su información y sus referencias siguen siendo válidas; no admite nuevos usos dentro del catálogo: no se puede asignar un producto a una categoría inactiva ni modificar la estructura, precios o marcas de un producto inactivo hasta reactivarlo (sí se puede consultar y reactivar). La baja implica no disponible y no visible al público (ver R-10) sin alterar la marca manual de las opciones.
- **Rationale**: FR-018 y las decisiones de clarificación.
- **Alternatives considered**: eliminación física con verificación de referencias (descartada por la clarificación).

## R-8. Pruebas, sin ampliar el stack obligatorio

- **Decision**: xUnit en backend (pruebas de dominio, de casos de uso con dobles de los puertos, e integración de la API contra una base dedicada en `psql-17`) y el ejecutor por defecto del proyecto Angular en frontend.
- **Rationale**: dependencias complementarias sin decisiones arquitectónicas contrarias a la constitución. Reutilizar `psql-17` evita agregar herramientas de contenedores que la constitución no requiere.
- **Alternatives considered**: bases en memoria (no reproducen restricciones reales de PostgreSQL); librerías de contenedores de prueba (dependencia adicional innecesaria).

## R-9. Imágenes ilustrativas

- **Decision**: el producto y cada opción guardan una referencia de imagen opcional en forma de texto (URL). Esta feature no sube, almacena ni transforma archivos. El catálogo indica si el producto tiene imagen.
- **Rationale**: FR-016 y FR-012 piden información de imagen para presentación posterior, no un mecanismo de carga. La imagen del producto es la que condiciona la visibilidad pública (R-10); la de la opción es informativa.
- **Alternatives considered**: carga de archivos con almacenamiento propio (amplía el alcance y decide almacenamiento sin requisito).

## R-10. Disponibilidad y visibilidad pública derivadas (FR-015, FR-016, FR-018)

- **Decision**: cada opción persiste una marca manual de disponibilidad, obligatoria al crearla (no se decide un valor por defecto). El catálogo calcula al leer, sin persistirlos:
  - **Disponibilidad efectiva de una opción** = marca manual, opción activa, producto activo y categoría activa.
  - **Disponibilidad del producto** = alguna de sus opciones con disponibilidad efectiva.
  - **Visibilidad pública del producto** = producto activo, categoría activa y producto con imagen.
  - **Visibilidad pública de una opción** = opción activa y visibilidad pública del producto.
  La baja o la reactivación nunca modifican la marca manual. La disponibilidad no se deriva del inventario.
- **Rationale**: implementa las decisiones de clarificación sin duplicar estados que pudieran desincronizarse. Los indicadores calculados quedan disponibles para POS y catálogo público, que decidirán cómo usarlos en sus propias features.
- **Alternatives considered**: persistir un indicador de visibilidad (riesgo de inconsistencia con la baja y la imagen).

## R-13. Retiro de opciones y referencias (FR-021)

- **Decision**: cada opción tiene un estado activa/inactiva. Se puede eliminar definitivamente solo si ninguna funcionalidad consumidora la referencia y no es la única opción del producto; en otro caso solo se da de baja (reversible). El catálogo consulta las referencias mediante un puerto de salida de la capa de aplicación, `IOptionReferenceChecker`. Hoy su única implementación (en Infrastructure) responde siempre "no referenciada", porque aún no existen consumidores.
- **Rationale**: respeta la decisión de clarificación sin acoplar el catálogo a POS, recetas o inventario, y cumple el Principio III (el consumidor real se agrega como adaptador).
- **Obligación futura**: cada feature que referencie opciones (POS, recetas, inventario) DEBE extender esa comprobación; de lo contrario se podrían eliminar opciones referenciadas. Las pruebas de integración usan un doble del puerto que responde "referenciada".
- **Alternatives considered**: consulta directa a tablas de otras features (acoplamiento); prohibir toda eliminación (descartado por la clarificación).

## R-11. Contrato HTTP administrativo

- **Decision**: API REST con JSON, controladores en el adaptador `Api`, todas las rutas bajo `/api/admin/catalog`, errores en formato ProblemDetails. Detalle en [contracts/catalog-admin-api.md](./contracts/catalog-admin-api.md).
- **Rationale**: prefijo administrativo único para aplicar autenticación y autorización de forma centralizada cuando exista la feature de usuarios y roles (Principio IV).
- **Alternatives considered**: reutilizar rutas sin prefijo (mezcla lo administrativo con una futura superficie pública).

## R-12. Frontend administrativo

- **Decision**: aplicación Angular 22 en `frontend/admin` con un módulo funcional `catalog` (páginas de categorías y productos, formulario de opciones, servicio HTTP y modelos). Interfaz con CoreUI for Angular Free sobre Bootstrap 5 y Minty.
- **Rationale**: es la organización mínima que cumple el stack visual; no define la arquitectura de otras features.
- **Alternatives considered**: monorepo con librerías compartidas entre frontends (prematuro; Principio V exige independencia).

## Cuestiones funcionales que siguen abiertas (no resueltas aquí)

- Cuestión 7 (parte de modificación): efecto de modificar una variante sobre referencias existentes. Por R-6 el identificador no cambia.
- Comparación de acentos en los nombres: hoy son significativos (R-5).
- Si el catálogo público mostrará productos no disponibles: lo define la feature del catálogo público.
