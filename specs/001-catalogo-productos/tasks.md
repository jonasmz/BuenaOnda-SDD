---

description: "Lista de tareas para la gestión del catálogo de productos"
---

# Tasks: Gestión del catálogo de productos

**Input**: documentos de diseño en `/specs/001-catalogo-productos/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/catalog-admin-api.md, quickstart.md

**Revisión 2026-09-21**: el catálogo pasó a ser plano (sin variantes; constitución v2.0.0). Las fases 1 a 3 no cambian; las fases 4 en adelante se rehicieron y la fase 4 incluye el retiro del código de variantes ya implementado.

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
- [X] T011 [P] Crear el fixture de integración en `APIT/CatalogApiFixture.cs` que crea la base dedicada en `psql-17`, aplica las migraciones y ofrece un `HttpClient` de la API; debe permitir reemplazar servicios registrados en pruebas (depende de T005, T009)

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

## Phase 4: User Story 2 - Registrar y consultar productos (Priority: P1)

**Goal**: registrar, consultar y modificar productos planos con precio y disponibilidad. Cada presentación o variedad es un producto distinto (FR-007).

**Independent Test**: registrar un producto en una categoría existente y verificar consulta y modificación (quickstart 2 a 9, 13, 14, 19, 20).

### Tests for User Story 2

- [X] T022 [P] [US2] Pruebas de dominio de `Product` plano en `DOMT/Catalog/ProductTests.cs`: nombre no vacío, precio obligatorio y no negativo, marca de disponibilidad obligatoria, imagen opcional y URL válida, categoría inactiva rechazada, indicadores derivados de disponibilidad y visibilidad pública (data-model.md)
- [X] T023 [P] [US2] Pruebas de integración en `APIT/Catalog/ProductsApiTests.cs`: crear producto sin imagen (visible al público = falso), asignar imagen, modificar precio, nombre repetido en la misma categoría → 409, mismo nombre en otra categoría → 201, categoría inactiva → 409, mover a una categoría donde ya existe el nombre → 409, datos inválidos → 400
- [X] T024 [P] [US2] Pruebas de casos de uso de productos (unicidad por categoría, categoría inactiva) con dobles de los puertos en `backend/tests/BuenaOnda.Application.Tests/Catalog/ProductUseCasesTests.cs`

### Implementation for User Story 2

- [X] T025 [US2] Reemplazar `DOM/Product.cs` por el agregado plano (Id, Nombre único en su categoría, Descripción, Imagen, Precio, Disponibilidad manual, Categoría, Activo) y actualizar el puerto `IProductRepository`; eliminar del dominio `SellableOption`, `VariationCharacteristic`, `OptionValue` y `OptionDraft`
- [X] T026 [US2] Reemplazar los casos de uso de productos en `APP/Products/` por `CreateProduct`, `GetProduct`, `ListProducts` y `UpdateProduct` planos, con `ProductView` con `isAvailable` e `isVisibleToPublic`; eliminar `IOptionReferenceChecker` y los casos de uso de opciones y características (FR-005, FR-006, FR-008, FR-009, FR-011)
- [X] T027 [US2] Reemplazar la configuración EF Core y el repositorio de productos en `INF/Products/` con índice único (categoría, nombre normalizado); eliminar `NoReferencesOptionReferenceChecker`; reemplazar las migraciones existentes por una única migración inicial del catálogo plano y recrear la base de desarrollo (depende de T025)
- [X] T028 [US2] Reemplazar `API/ProductsController.cs` por `GET /products`, `POST /products`, `GET /products/{id}` y `PUT /products/{id}` planos, y ajustar los DTOs (depende de T026)
- [X] T029 [P] [US2] Reemplazar los modelos y métodos de productos en `FE/models/product.ts` y `FE/services/catalog-api.service.ts`; eliminar `FE/components/variation-editor/` y `FE/components/option-form/` con sus pruebas
- [X] T030 [US2] Ajustar las páginas de listado y formulario de productos en `FE/pages/products/` a productos planos: precio, disponibilidad, "sin imagen" e indicadores (depende de T029, T028)

**Checkpoint**: las historias 1 y 2 funcionan con catálogo plano; no queda código de variantes.

---

## Phase 5: User Story 3 - Modificar, dar de baja y reactivar el catálogo (Priority: P2)

**Goal**: marcar la disponibilidad de productos y dar de baja y reactivar categorías y productos (FR-010, FR-013).

**Independent Test**: modificar una categoría y un producto, marcar disponibilidad, dar de baja y reactivar (quickstart 8, 10 a 12, 15 a 17).

### Tests for User Story 3

- [X] T031 [P] [US3] Pruebas de dominio en `DOMT/Catalog/CatalogLifecycleTests.cs`: baja y reactivación de categoría y producto, producto inactivo o de categoría inactiva no admite modificaciones (invariante 5), la baja no altera la marca manual (invariante 6), indicadores derivados tras la baja
- [X] T032 [P] [US3] Pruebas de integración en `APIT/Catalog/CatalogModificationApiTests.cs`: fijar disponibilidad, modificar una categoría conserva la asociación de sus productos, baja de categoría con productos, reactivación conserva marcas, modificar un producto inactivo → 409, no existe endpoint de eliminación de productos ni categorías

### Implementation for User Story 3

- [X] T033 [US3] Implementar en `DOM/Product.cs` y `DOM/Category.cs` `Deactivate`, `Reactivate` y `SetAvailability`, con la protección del invariante 5 y sin eliminación definitiva
- [X] T034 [US3] Crear los casos de uso `DeactivateCategory`, `ReactivateCategory`, `DeactivateProduct`, `ReactivateProduct` y `SetProductAvailability` en `APP/Categories/` y `APP/Products/` (depende de T033)
- [X] T035 [US3] Agregar los endpoints `POST /categories/{id}/deactivate|reactivate`, `POST /products/{id}/deactivate|reactivate` y `PUT /products/{id}/availability` (depende de T034)
- [X] T036 [US3] Agregar en `FE/pages/categories/` y `FE/pages/products/` las acciones de dar de baja y reactivar y el conmutador de disponibilidad, con estado visible (depende de T030, T035)

**Checkpoint**: las historias 1 a 3 funcionan.

---

## Phase 6: User Story 4 - Extensibilidad del catálogo (Priority: P3)

**Goal**: demostrar que categorías y productos nuevos (por ejemplo tragos) no requieren cambiar los conceptos generales.

**Independent Test**: definir una categoría y productos distintos a los actuales usando solo los conceptos existentes (quickstart 7). Los tragos NO se incorporan al alcance actual.

- [X] T037 [P] [US4] Prueba de integración en `APIT/Catalog/CatalogExtensibilityTests.cs`: crear la categoría "Tragos" y el producto "Fernet con cola" sin modificar código ni migraciones (FR-012, SC-005)
- [X] T038 [P] [US4] Prueba de integración en `APIT/Catalog/CurrentProductsRepresentationTests.cs`: representar todos los productos de `system_requirements.txt` §9 como productos planos según data-model.md (SC-001, SC-002)

**Checkpoint**: las cuatro historias funcionan.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T039 [P] Crear la prueba de arquitectura en `DOMT/ArchitectureTests.cs` que verifique por reflexión que los ensamblados Domain y Application no referencian Entity Framework Core ni Npgsql y que Domain no referencia otros proyectos (Principio III)
- [X] T040 [P] Revisar que ningún proyecto, ruta ni DTO introduzca conceptos fuera de alcance (variantes, pedidos, inventario, recetas, clientes, sucursales) buscando en `backend/` y `frontend/admin/` (Principios II y VIII)
- [ ] T041 Ejecutar los 20 escenarios de `specs/001-catalogo-productos/quickstart.md` contra la API y la interfaz y corregir las diferencias
- [ ] T042 Verificar las condiciones de la excepción transitoria del Principio IV (constitución): todos los endpoints cuelgan de `/api/admin/catalog`, no existe ningún artefacto de despliegue o exposición fuera del entorno de desarrollo, y el cierre queda registrado en Complexity Tracking de `specs/001-catalogo-productos/plan.md`

---

## Dependencies & Execution Order

- **Setup (Phase 1)** y **Foundational (Phase 2)**: sin cambios; ya completadas.
- **US1 (Phase 3)**: completada.
- **US2 (Phase 4)** depende de US1 (necesita categorías) y reemplaza el código de variantes.
- **US3 (Phase 5)** depende de US2.
- **US4 (Phase 6)** depende de US2.
- **Polish (Phase 7)** depende de las historias deseadas.
- Dentro de cada historia: las pruebas se escriben primero; dominio → casos de uso → persistencia y migración → API → frontend.

## Implementation Strategy

Historias 1 y 2 dejan el catálogo utilizable por los consumidores futuros; la 3 agrega disponibilidad y baja; la 4 verifica extensibilidad.

## Notes

- La autenticación y autorización no forman parte de esta feature: se ampara en la excepción transitoria del Principio IV (plan.md, Complexity Tracking). Esta feature NO DEBE desplegarse ni exponerse fuera del entorno de desarrollo; el prefijo `/api/admin/catalog` es el punto donde la feature de usuarios y roles aplicará la protección.
- Cuestiones abiertas que NO se implementan: efecto de modificar un producto sobre referencias existentes, comparación de acentos.
- Confirmar el commit de cada tarea o grupo lógico antes de continuar.
