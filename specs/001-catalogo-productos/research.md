# Research: Gestión del catálogo de productos

Decisiones técnicas de Phase 0. No queda ningún `NEEDS CLARIFICATION` del Technical Context; las cuestiones funcionales abiertas de la spec se listan al final y NO se resuelven aquí.

## R-1. Estructura hexagonal de la solución backend

- **Decision**: cuatro proyectos: `Domain` (sin referencias a otros proyectos ni paquetes externos), `Application` (referencia a Domain; define los puertos de salida), `Infrastructure` (referencia a Application; implementa persistencia con EF Core y Npgsql) y `Api` (adaptador de entrada; referencia a Application; componer dependencias en el arranque).
- **Rationale**: cumple el Principio III y hace verificable por referencias de proyecto que dominio y casos de uso no ven EF Core. El proyecto `Api` referencia a `Infrastructure` solo para el registro de dependencias (composition root).
- **Alternatives considered**: un único proyecto con carpetas (no hace cumplir la dirección de dependencias por compilación); proyectos por módulo funcional (innecesario para una sola feature).

## R-2. Catálogo plano: Producto y Categoría como agregados simples

- **Decision**: `Producto` y `Categoría` son agregados independientes y sin entidades hijas. El producto lleva su nombre, descripción, imagen, precio, marca manual de disponibilidad, categoría y estado. No existen variantes, opciones ni características de variación (constitución v2.0.0, Principio VIII).
- **Rationale**: FR-007 y la decisión del responsable del proyecto: las variantes eran complejidad innecesaria. Cada presentación o variedad vendida por separado es un producto, por lo que POS, recetas e inventario referencian siempre un único concepto estable: el identificador de producto (FR-014).
- **Alternatives considered**: producto con variantes u opciones (descartado por decisión expresa); agrupar productos afines para mostrarlos juntos (descartado: no se agrupan ni se relacionan).

## R-3. Precio y disponibilidad en el producto

- **Decision**: el precio (decimal no negativo, sin historial) y la marca manual de disponibilidad pertenecen al producto. La marca es obligatoria al crearlo (no se decide un valor por defecto).
- **Rationale**: FR-008, FR-009 y FR-010; un único precio por producto es no ambiguo para los consumidores.
- **Alternatives considered**: precio base con ajustes (complejidad sin requisito).

## R-5. Unicidad (FR-015)

- **Decision**: (a) el nombre de una categoría es único entre categorías; (b) el nombre de un producto es único dentro de su categoría, incluso al moverlo a otra. La comparación ignora mayúsculas y espacios de borde (los acentos siguen siendo significativos). Se valida en el dominio y se refuerza con restricciones de unicidad en la base de datos sobre la forma normalizada.
- **Rationale**: FR-015 define las reglas; el refuerzo en base de datos evita duplicados por concurrencia.
- **Alternatives considered**: solo validar en la aplicación (permite duplicados bajo concurrencia).

## R-6. Identificadores estables para los consumidores

- **Decision**: cada categoría y producto tiene un identificador único inmutable, y editar su información nunca lo cambia.
- **Rationale**: FR-014 y FR-013 (las referencias existentes siguen válidas). Los consumidores futuros referencian productos por identificador.
- **Alternatives considered**: usar el nombre como clave (frágil ante ediciones).

## R-7. Baja reversible

- **Decision**: categorías y productos tienen un estado activo/inactivo. No hay operación de eliminación definitiva. Un elemento inactivo conserva su información y sus referencias siguen siendo válidas; no admite nuevos usos dentro del catálogo: no se puede asignar un producto a una categoría inactiva ni modificar la información, el precio o la marca de un producto inactivo hasta reactivarlo (sí se puede consultar y reactivar). La baja implica no disponible y no visible al público (ver R-10) sin alterar la marca manual de disponibilidad del producto.
- **Rationale**: FR-013 y las decisiones de clarificación.
- **Alternatives considered**: eliminación física con verificación de referencias (descartada por la clarificación).

## R-8. Pruebas, sin ampliar el stack obligatorio

- **Decision**: xUnit en backend (pruebas de dominio, de casos de uso con dobles de los puertos, e integración de la API contra una base dedicada en `psql-17`) y el ejecutor por defecto del proyecto Angular en frontend.
- **Rationale**: dependencias complementarias sin decisiones arquitectónicas contrarias a la constitución. Reutilizar `psql-17` evita agregar herramientas de contenedores que la constitución no requiere.
- **Alternatives considered**: bases en memoria (no reproducen restricciones reales de PostgreSQL); librerías de contenedores de prueba (dependencia adicional innecesaria).

## R-9. Imágenes ilustrativas

- **Decision**: el producto guarda una referencia de imagen opcional en forma de texto (URL). Esta feature no sube, almacena ni transforma archivos. El catálogo indica si el producto tiene imagen.
- **Rationale**: FR-011 pide información de imagen para presentación posterior, no un mecanismo de carga. La imagen condiciona la visibilidad pública (R-10).
- **Alternatives considered**: carga de archivos con almacenamiento propio (amplía el alcance y decide almacenamiento sin requisito).

## R-10. Disponibilidad y visibilidad pública derivadas (FR-010, FR-011, FR-013)

- **Decision**: cada producto persiste una marca manual de disponibilidad, obligatoria al crearlo. El catálogo calcula al leer, sin persistirlos:
  - **Disponibilidad efectiva** = marca manual, producto activo y categoría activa.
  - **Visibilidad pública** = producto activo, categoría activa y producto con imagen.
  La baja o la reactivación nunca modifican la marca manual. La disponibilidad no se deriva del inventario.
- **Rationale**: implementa las decisiones de clarificación sin duplicar estados que pudieran desincronizarse. Los indicadores quedan disponibles para POS y catálogo público, que decidirán cómo usarlos en sus propias features.
- **Alternatives considered**: persistir un indicador de visibilidad (riesgo de inconsistencia con la baja y la imagen).

## R-11. Contrato HTTP administrativo

- **Decision**: API REST con JSON, controladores en el adaptador `Api`, todas las rutas bajo `/api/admin/catalog`, errores en formato ProblemDetails. Detalle en [contracts/catalog-admin-api.md](./contracts/catalog-admin-api.md).
- **Rationale**: prefijo administrativo único para aplicar autenticación y autorización de forma centralizada cuando exista la feature de usuarios y roles (Principio IV).
- **Alternatives considered**: reutilizar rutas sin prefijo (mezcla lo administrativo con una futura superficie pública).

## R-12. Frontend administrativo

- **Decision**: aplicación Angular 22 en `frontend/admin` con un módulo funcional `catalog` (páginas de categorías y productos, servicio HTTP y modelos). Interfaz con CoreUI for Angular Free sobre Bootstrap 5 y Minty.
- **Rationale**: es la organización mínima que cumple el stack visual; no define la arquitectura de otras features.
- **Alternatives considered**: monorepo con librerías compartidas entre frontends (prematuro; Principio V exige independencia).

## Cuestiones funcionales que siguen abiertas (no resueltas aquí)

- Efecto de modificar un producto (por ejemplo su precio) sobre referencias existentes. Por R-6 el identificador no cambia.
- Comparación de acentos en los nombres: hoy son significativos (R-5).
- Si el catálogo público mostrará productos no disponibles: lo define la feature del catálogo público.
