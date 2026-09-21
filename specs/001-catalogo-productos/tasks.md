---

description: "Lista de tareas para la gestión del catálogo de productos"
---

# Tasks: Gestión del catálogo de productos

**Input**: documentos de diseño en `/specs/001-catalogo-productos/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/catalog-admin-api.md, quickstart.md

**Tests**: se incluyen porque el plan (Technical Context y Project Structure) declara xUnit y proyectos de pruebas, y el quickstart indica ejecutarlas. Son pruebas funcionales de dominio, de casos de uso y de la API; no se fijan métricas de cobertura.

**Organization**: las tareas se agrupan por historia de usuario para poder implementarlas y probarlas de forma independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: se puede ejecutar en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: historia de usuario a la que pertenece (US1 a US5)
- Cada tarea indica la ruta exacta del archivo

## Path Conventions

Aplicación web según plan.md: `backend/` (solución .NET hexagonal) y `frontend/admin/` (Angular 22). Abreviaturas de rutas:

- `DOM` = `backend/src/BuenaOnda.Domain/Catalog`
- `APP` = `backend/src/BuenaOnda.Application/Catalog`
- `INF` = `backend/src/BuenaOnda.Infrastructure/Persistence`
- `API` = `backend/src/BuenaOnda.Api/Catalog`
- `FE` = `frontend/admin/src/app/catalog`
- `DOMT` = `backend/tests/BuenaOnda.Domain.Tests`
- `APIT` = `backend/tests/BuenaOnda.Api.IntegrationTests`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: esqueleto del repositorio según plan.md

- [X] T001 Crear `backend/BuenaOndaSK.sln` y los proyectos `BuenaOnda.Domain`, `BuenaOnda.Application`, `BuenaOnda.Infrastructure` y `BuenaOnda.Api` sobre .NET 10 con referencias Application→Domain, Infrastructure→Application, Api→Application y Api→Infrastructure (esta última solo para el registro de dependencias); Domain NO DEBE referenciar ningún paquete ni proyecto externo (research R-1)
- [X] T002 [P] Crear los proyectos xUnit `backend/tests/BuenaOnda.Domain.Tests`, `backend/tests/BuenaOnda.Application.Tests` y `backend/tests/BuenaOnda.Api.IntegrationTests` y agregarlos a la solución
- [X] T003 [P] Agregar los paquetes Entity Framework Core y el proveedor Npgsql a `backend/src/BuenaOnda.Infrastructure/BuenaOnda.Infrastructure.csproj`, y las herramientas de diseño de EF Core al proyecto de arranque `backend/src/BuenaOnda.Api/BuenaOnda.Api.csproj`
- [X] T004 [P] Crear la aplicación Angular 22 en `frontend/admin` e instalar y configurar Bootstrap 5, CoreUI for Angular Free, el tema Bootswatch Minty y Font Awesome Free en `frontend/admin/src/styles.scss`
- [X] T005 [P] Definir la cadena de conexión a PostgreSQL 17 del contenedor `psql-17` en `backend/src/BuenaOnda.Api/appsettings.Development.json` sin credenciales versionadas (usar variable de entorno o user-secrets) y una base dedicada para pruebas de integración

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: infraestructura común que DEBE completarse antes de cualquier historia

- [X] T006 [P] Crear en `DOM/NormalizedName.cs` la normalización de nombres (ignora mayúsculas y espacios de borde; los acentos siguen siendo significativos) y en `backend/src/BuenaOnda.Domain/Common/` las excepciones de dominio `ValidationException`, `NotFoundException` y `ConflictException`
- [X] T007 [P] Definir en `APP/Ports/` el puerto de salida `IUnitOfWork`; `ICategoryRepository` e `IProductRepository` se declaran junto a sus entidades en T015 y T025, porque dependen de ellas y de lo contrario este checkpoint no compilaría
- [X] T008 Crear `INF/CatalogDbContext.cs` y el método de registro de servicios de Infrastructure en `backend/src/BuenaOnda.Infrastructure/DependencyInjection.cs` (depende de T003, T007)
- [X] T009 Configurar el arranque en `backend/src/BuenaOnda.Api/Program.cs`: composición de dependencias, prefijo de rutas `/api/admin/catalog` en `API/CatalogControllerBase.cs` y mapeo de excepciones de dominio a ProblemDetails (Validation→400, NotFound→404, Conflict→409) en `backend/src/BuenaOnda.Api/ProblemDetailsMapping.cs` (depende de T006, T008)
- [X] T010 [P] Crear el shell administrativo Angular: rutas, layout CoreUI, configuración de la URL base de la API y el módulo `catalog` vacío en `frontend/admin/src/app/app.routes.ts` y `FE/catalog.routes.ts` (depende de T004)
- [X] T011 [P] Crear el fixture de integración en `APIT/CatalogApiFixture.cs` que crea la base dedicada en `psql-17`, aplica las migraciones y ofrece un `HttpClient` de la API; debe permitir reemplazar servicios registrados, por ejemplo `IOptionReferenceChecker`, en pruebas (depende de T005, T009)

**Checkpoint**: la base está lista; se pueden implementar las historias.

---

## Phase 3: User Story 1 - Administrar categorías (Priority: P1) 🎯 MVP

**Goal**: crear, consultar y modificar categorías.

**Independent Test**: crear, consultar y modificar categorías sin que existan productos (quickstart 1 y 19).

### Tests for User Story 1

- [X] T012 [P] [US1] Pruebas de dominio de `Category` en `DOMT/Catalog/CategoryTests.cs`: nombre obligatorio y no vacío, creación activa, modificación de nombre y descripción
- [X] T013 [P] [US1] Pruebas de integración en `APIT/Catalog/CategoriesApiTests.cs`: crear, listar, consultar, modificar, categoría sin productos consultable, nombre repetido con distinta capitalización → 409 (FR-020)
- [X] T014 [P] [US1] Pruebas de casos de uso de categorías con dobles de `ICategoryRepository` en `backend/tests/BuenaOnda.Application.Tests/Catalog/CategoryUseCasesTests.cs`

### Implementation for User Story 1

- [X] T015 [P] [US1] Crear la entidad `Category` y declarar el puerto `ICategoryRepository` en `APP/Ports/`; entidad `Category` en `DOM/Category.cs` con Id (GUID inmutable), Nombre ("Obligatorio, no vacío; único entre categorías (sin distinguir mayúsculas ni espacios de borde)"), Descripción (opcional) y Activa
- [X] T016 [US1] Crear los casos de uso `CreateCategory`, `UpdateCategory`, `GetCategory` y `ListCategories` (con `includeInactive`) en `APP/Categories/`, verificando la unicidad del nombre normalizado mediante `ICategoryRepository` (FR-001 a FR-003, FR-020) (depende de T015)
- [X] T017 [US1] Implementar la configuración EF Core `CategoryConfiguration` con índice único sobre el nombre normalizado y `CategoryRepository` en `INF/Categories/` (depende de T015, T008)
- [X] T018 [US1] Generar la migración inicial de categorías en `backend/src/BuenaOnda.Infrastructure/Migrations/` (depende de T017)
- [X] T019 [US1] Crear `API/CategoriesController.cs` y sus DTOs con `GET /categories`, `POST /categories`, `GET /categories/{id}` y `PUT /categories/{id}` según contracts/catalog-admin-api.md (depende de T016, T009)
- [X] T020 [P] [US1] Crear los modelos y el servicio HTTP de categorías en `FE/models/category.ts` y `FE/services/catalog-api.service.ts` (depende de T010)
- [X] T021 [US1] Crear las páginas de listado y formulario de categorías en `FE/pages/categories/` y sus rutas (depende de T020, T019)

**Checkpoint**: la historia 1 funciona de forma independiente (MVP).

---

## Phase 4: User Story 2 - Registrar productos y su información comercial (Priority: P1)

**Goal**: registrar, consultar y modificar productos con su información comercial. Un producto sin características tiene exactamente una opción con su propio precio (FR-008, FR-013).

**Independent Test**: registrar un producto en una categoría existente y verificar consulta y modificación (quickstart 7, 13, 14, 20).

### Tests for User Story 2

- [X] T022 [P] [US2] Pruebas de dominio de `Product` en `DOMT/Catalog/ProductTests.cs`: nombre no vacío, producto sin características con exactamente una opción, precio no negativo, imagen opcional, indicadores derivados de disponibilidad y visibilidad pública (data-model.md)
- [X] T023 [P] [US2] Pruebas de integración en `APIT/Catalog/ProductsApiTests.cs`: crear producto sin imagen (visible al público = falso), asignar imagen, nombre repetido en la misma categoría → 409, mismo nombre en otra categoría → 201, categoría inactiva → 409, mover un producto a otra categoría donde ya existe ese nombre → 409, modificar información comercial
- [X] T024 [P] [US2] Pruebas de casos de uso de productos (unicidad por categoría, categoría inactiva) con dobles de los puertos en `backend/tests/BuenaOnda.Application.Tests/Catalog/ProductUseCasesTests.cs`

### Implementation for User Story 2

- [X] T025 [P] [US2] Declarar el puerto `IProductRepository` en `APP/Ports/` y crear el agregado `Product` en `DOM/Product.cs` con Id (GUID inmutable), Nombre ("Obligatorio, no vacío; único dentro de su categoría"), Descripción (opcional), Imagen (URL opcional; "sin imagen el producto no es visible al público"), Categoría ("Obligatoria; debe estar activa al asignarla") y Activo
- [X] T026 [US2] Crear `SellableOption` en `DOM/SellableOption.cs` con Id (GUID inmutable), Precio ("Obligatorio; decimal no negativo; moneda única del establecimiento"), Disponibilidad manual ("Obligatoria al crear la opción"), Descripción (opcional), Imagen (URL opcional; "informativa, no condiciona la visibilidad del producto") y Activa ("Se puede dar de baja y reactivar; solo se elimina definitivamente si nunca fue referenciada y no es la última opción del producto")
- [X] T027 [US2] Crear los casos de uso `CreateProduct` (sin características), `GetProduct`, `ListProducts` (por categoría e `includeInactive`) y `UpdateProduct` (nombre, descripción, imagen, categoría) en `APP/Products/`, más `ProductView` con `isAvailable` e `isVisibleToPublic` calculados para el producto y para cada opción (Disponibilidad efectiva = marca manual, opción activa, producto activo y categoría activa; visibilidad del producto = producto activo, categoría activa y con imagen; visibilidad de la opción = opción activa y visibilidad del producto) (FR-005, FR-006, FR-016) (depende de T025, T026)
- [X] T028 [US2] Implementar `ProductConfiguration` con índice único (categoría, nombre normalizado) y `ProductRepository` en `INF/Products/` (depende de T025, T026, T017)
- [X] T029 [US2] Generar la migración de productos y opciones en `backend/src/BuenaOnda.Infrastructure/Migrations/` (depende de T028)
- [X] T030 [US2] Crear `API/ProductsController.cs` y DTOs con `GET /products`, `POST /products`, `GET /products/{id}` y `PUT /products/{id}` (depende de T027)
- [X] T031 [P] [US2] Crear modelos y métodos de productos en `FE/models/product.ts` y `FE/services/catalog-api.service.ts` (depende de T010)
- [X] T032 [US2] Crear las páginas de listado y formulario de productos en `FE/pages/products/`, indicando "sin imagen" y los indicadores de disponibilidad y visibilidad (depende de T031, T030)

**Checkpoint**: las historias 1 y 2 funcionan.

---

## Phase 5: User Story 3 - Definir variantes flexibles de un producto (Priority: P1)

**Goal**: definir características de variación y opciones comercializables sin estructuras por producto.

**Independent Test**: representar los escenarios 1 a 5 (papas, hamburguesa, pizza, bebida, agua saborizada) sin tipos específicos (quickstart 2 a 6, 9).

### Tests for User Story 3

- [X] T033 [P] [US3] Pruebas de dominio en `DOMT/Catalog/ProductVariationTests.cs`: escenarios 1 a 5, opción indistinguible rechazada (invariante 3), característica repetida rechazada (invariante 4), valores incompletos o de más rechazados (invariante 2), agregar y quitar característica (invariante 5), producto con una sola opción consultable y seleccionable
- [X] T034 [P] [US3] Pruebas del servicio HTTP y del editor de variación en `frontend/admin/src/app/catalog/components/variation-editor/variation-editor.spec.ts`
- [X] T035 [P] [US3] Pruebas de integración en `APIT/Catalog/ProductVariationApiTests.cs`: creación con características y opciones, agregar opción, agregar y quitar característica, duplicado → 409, 500 ml/750 ml/1 litro/1,5 litros más una presentación nueva

### Implementation for User Story 3

- [X] T036 [P] [US3] Crear `VariationCharacteristic` en `DOM/VariationCharacteristic.cs` con Nombre ("Único dentro del producto, sin distinguir mayúsculas ni espacios de borde") y el cálculo de la firma normalizada de valores de una opción en `DOM/OptionSignature.cs`
- [X] T037 [US3] Extender `Product` en `DOM/Product.cs` con la creación con características y opciones, `AddOption`, `AddCharacteristic` (aportando el valor para cada opción existente) y `RemoveCharacteristic` (solo si las opciones siguen distinguibles), garantizando los invariantes 1 a 5 (depende de T036)
- [X] T038 [US3] Crear los casos de uso `AddOption`, `AddCharacteristic` y `RemoveCharacteristic` en `APP/Products/` y extender `CreateProduct` para aceptar características y opciones (FR-008 a FR-011) (depende de T037)
- [X] T039 [US3] Mapear características y valores de opción en `INF/Products/` con índice único (producto, firma normalizada) para reforzar FR-011 (depende de T037, T028)
- [X] T040 [US3] Generar la migración de variación en `backend/src/BuenaOnda.Infrastructure/Migrations/` (depende de T039)
- [X] T041 [US3] Extender `API/ProductsController.cs` con `POST /products/{id}/options`, `POST /products/{id}/characteristics` y `DELETE /products/{id}/characteristics/{characteristicId}`, y aceptar `characteristics` y `options` en `POST /products` (depende de T038)
- [X] T042 [P] [US3] Crear el componente editor de variación en `FE/components/variation-editor/` (características dinámicas y matriz de opciones con precio y marca de disponibilidad) (depende de T031)
- [X] T043 [US3] Integrar el editor de variación en el formulario y en la vista de detalle de producto de `FE/pages/products/` mostrando sin ambigüedad todas las opciones (depende de T042, T041)

**Checkpoint**: las historias 1 a 3 funcionan; los productos actuales ya se pueden representar.

---

## Phase 6: User Story 4 - Modificar el catálogo existente (Priority: P2)

**Goal**: modificar opciones y precios, marcar disponibilidad manual, dar de baja y reactivar categorías, productos y opciones, y retirar opciones (FR-021).

**Independent Test**: modificar una categoría, un producto y una opción y consultar el resultado; dar de baja y reactivar; eliminar o dar de baja una opción (quickstart 10 a 12, 15 a 18, 21 a 25).

### Tests for User Story 4

- [ ] T044 [P] [US4] Pruebas de dominio en `DOMT/Catalog/CatalogLifecycleTests.cs`: baja y reactivación de categoría y producto, producto inactivo o de categoría inactiva no admite modificaciones (invariante 7), la baja no altera la marca manual (invariante 9), indicadores derivados tras la baja, baja y reactivación de una opción, eliminación de opción solo si no fue referenciada y no es la última (invariantes 10 y 11), valores de una opción inactiva siguen reservados (invariante 12)
- [ ] T045 [P] [US4] Pruebas de integración en `APIT/Catalog/CatalogModificationApiTests.cs`: modificar precio sin historial, modificar solo una opción, fijar disponibilidad por opción, producto no disponible cuando ninguna opción lo está, modificar una categoría conserva la asociación de sus productos, baja de categoría con productos, reactivación conserva marcas, no existe endpoint de eliminación de productos ni categorías, eliminar opción no referenciada → 204, eliminar la última opción → 409, eliminar opción referenciada (usando un doble de `IOptionReferenceChecker` que responde "referenciada") → 409 y baja disponible, recrear los valores de una opción inactiva → 409

### Implementation for User Story 4

- [ ] T046 [P] [US4] Definir el puerto `IOptionReferenceChecker` en `APP/Ports/IOptionReferenceChecker.cs` y su implementación por defecto `NoReferencesOptionReferenceChecker` (responde siempre "no referenciada", porque aún no hay funcionalidades consumidoras) en `INF/Products/`, registrada en `backend/src/BuenaOnda.Infrastructure/DependencyInjection.cs` (research R-13) (depende de T008)
- [ ] T047 [US4] Implementar en `DOM/Product.cs` y `DOM/Category.cs` `Deactivate`, `Reactivate`, `UpdateOption` (valores, precio, descripción, imagen), `SetOptionAvailability`, `DeactivateOption`, `ReactivateOption` y `RemoveOption`, con la protección de invariante 7, sin eliminación definitiva de productos ni categorías, y garantizando los invariantes 10 a 12 (la eliminación de una opción exige que quien llama informe que no está referenciada) (depende de T037)
- [ ] T048 [US4] Crear los casos de uso `DeactivateCategory`, `ReactivateCategory`, `DeactivateProduct`, `ReactivateProduct`, `UpdateOption`, `SetOptionAvailability`, `DeactivateOption`, `ReactivateOption` y `DeleteOption` (este último consulta `IOptionReferenceChecker`) en `APP/Categories/` y `APP/Products/` (FR-012, FR-014, FR-015, FR-018, FR-021) (depende de T047, T046)
- [ ] T049 [US4] Agregar los endpoints `POST /categories/{id}/deactivate|reactivate`, `POST /products/{id}/deactivate|reactivate`, `PUT /products/{id}/options/{optionId}`, `PUT /products/{id}/options/{optionId}/availability`, `POST /products/{id}/options/{optionId}/deactivate|reactivate` y `DELETE /products/{id}/options/{optionId}` en `API/CategoriesController.cs` y `API/ProductsController.cs` (depende de T048)
- [ ] T050 [P] [US4] Agregar en `FE/pages/categories/` y `FE/pages/products/` las acciones de dar de baja y reactivar, con estado visible (depende de T021, T032)
- [ ] T051 [US4] Crear el formulario de edición de opción en `FE/components/option-form/` con precio, descripción, imagen, conmutador de disponibilidad manual y acciones de dar de baja, reactivar y eliminar la opción (mostrando el conflicto 409 con la opción de baja), con su prueba en `FE/components/option-form/option-form.spec.ts` (depende de T043, T049)

**Checkpoint**: las historias 1 a 4 funcionan.

---

## Phase 7: User Story 5 - Extensibilidad del catálogo (Priority: P3)

**Goal**: demostrar que categorías y productos nuevos (por ejemplo tragos) no requieren cambiar los conceptos generales.

**Independent Test**: definir una categoría y productos distintos a los actuales usando solo los conceptos existentes (quickstart 8). Los tragos NO se incorporan al alcance actual.

- [ ] T052 [P] [US5] Prueba de integración en `APIT/Catalog/CatalogExtensibilityTests.cs`: crear la categoría "Tragos" y un producto de ejemplo con una característica nueva ("graduación") sin modificar código ni migraciones (FR-017, SC-005)
- [ ] T053 [P] [US5] Prueba de integración en `APIT/Catalog/CurrentProductsRepresentationTests.cs`: representar todos los productos de `system_requirements.txt` §9 (papas fritas, milanesas, hamburguesas, pizzas, gaseosas con un producto por marca, agua mineral, cerveza, agua saborizada) con las opciones indicadas en data-model.md (SC-001, SC-002)

**Checkpoint**: las cinco historias funcionan.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T054 [P] Crear la prueba de arquitectura en `DOMT/ArchitectureTests.cs` que verifique por reflexión que los ensamblados Domain y Application no referencian Entity Framework Core ni Npgsql y que Domain no referencia otros proyectos (Principio III)
- [ ] T055 [P] Revisar que ningún proyecto, ruta ni DTO introduzca conceptos fuera de alcance (pedidos, inventario, recetas, clientes, sucursales) buscando en `backend/` y `frontend/admin/` (Principio II)
- [ ] T056 Ejecutar los 25 escenarios de `specs/001-catalogo-productos/quickstart.md` contra la API y la interfaz (el 25 solo se cubre con la prueba automatizada) y corregir las diferencias
- [ ] T057 Verificar las condiciones de la excepción transitoria del Principio IV (constitución v1.2.0): todos los endpoints cuelgan de `/api/admin/catalog`, no existe ningún artefacto de despliegue o exposición fuera del entorno de desarrollo, y el cierre queda registrado en Complexity Tracking de `specs/001-catalogo-productos/plan.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias.
- **Foundational (Phase 2)**: depende de Setup; bloquea todas las historias.
- **US1 (Phase 3)** depende de Foundational.
- **US2 (Phase 4)** depende de US1 (necesita categorías).
- **US3 (Phase 5)** depende de US2 (extiende el agregado `Product`).
- **US4 (Phase 6)** depende de US3 para el editor de opciones (T051) y de US2 para lo demás; los casos de uso de categorías pueden empezar tras US1.
- **US5 (Phase 7)** depende de US3.
- **Polish (Phase 8)** depende de las historias deseadas.

### Within Each User Story

- Las pruebas se escriben primero y deben fallar antes de implementar.
- Dominio → casos de uso → persistencia y migración → API → frontend.

### Parallel Opportunities

- Setup: T002 a T005 en paralelo tras T001.
- Foundational: T006, T007, T010 en paralelo; T011 tras T005 y T009.
- US1: T012 y T013 en paralelo; T015 y T020 en paralelo.
- US2: T022 y T023; T025 y T026; T031 en paralelo con el backend.
- US3: T033 y T035; T036 y T042.
- US4: T044 y T045 en paralelo; T046 en paralelo con las pruebas.
- US5: T052 y T053 en paralelo.

### Parallel Example: User Story 1

```text
T012 Pruebas de dominio de Category
T013 Pruebas de integración de categorías
T015 Entidad Category
T020 Servicio HTTP de categorías en el frontend
```

---

## Implementation Strategy

### MVP First

1. Completar Phases 1 y 2.
2. Completar la historia 1 (categorías) y validarla.
3. Continuar con las historias 2 y 3: juntas dejan el catálogo con productos y variantes utilizable por los consumidores futuros.

### Incremental Delivery

Cada historia agrega valor sin romper las anteriores: US1 categorías → US2 productos → US3 variantes → US4 modificación, disponibilidad y baja → US5 verificación de extensibilidad.

---

## Notes

- La autenticación y autorización no forman parte de esta feature: se ampara en la excepción transitoria del Principio IV (constitución v1.2.0; plan.md, Complexity Tracking). Esta feature NO DEBE desplegarse ni exponerse fuera del entorno de desarrollo; el prefijo `/api/admin/catalog` es el punto donde la feature de usuarios y roles aplicará la protección.
- Cuestiones abiertas que NO se implementan: efecto de modificar una variante sobre referencias existentes, comparación de acentos.
- Obligación futura (research R-13): cada feature que referencie opciones (POS, recetas, inventario) DEBE extender `IOptionReferenceChecker`; de lo contrario podrían eliminarse opciones referenciadas.
- Confirmar el commit de cada tarea o grupo lógico antes de continuar.
