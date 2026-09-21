# Plan de implementación: Gestión del catálogo de productos

**Branch**: `001-catalogo-productos` | **Date**: 2026-09-21 | **Spec**: [spec.md](./spec.md)

**Input**: Especificación funcional en `/specs/001-catalogo-productos/spec.md`

## Summary

Se implementa la gestión administrativa del catálogo comercial (categorías, productos, variantes y precios) definida en la especificación. El repositorio no contiene todavía código de aplicación, por lo que esta feature crea el esqueleto mínimo necesario: una solución backend .NET con arquitectura hexagonal y una aplicación Angular administrativa con el módulo de catálogo.

Enfoque técnico (detalle en [research.md](./research.md) y [data-model.md](./data-model.md)):

- El producto es un agregado que posee sus **características de variación** y sus **opciones comercializables** (variantes). Toda opción vendible, incluso la de un producto sin variantes, es una opción con su propio precio, de modo que POS, recetas e inventario referencian siempre un único concepto estable (FR-013, FR-019).
- Las características y sus valores son **datos**, no tipos ni columnas del modelo: los productos actuales y una futura categoría de tragos usan el mismo modelo (FR-007, FR-009, FR-017).
- La baja de categorías y productos es un estado reversible; no existe eliminación definitiva (FR-018). Las opciones también tienen baja reversible y solo se eliminan si nunca fueron referenciadas y no son las últimas del producto; las referencias se consultan mediante un puerto de salida sin consumidores por ahora (FR-021, research R-13). La disponibilidad se marca a mano por opción (FR-015); la disponibilidad efectiva y la visibilidad pública se calculan al leer a partir de la baja, la marca manual y la imagen.
- La API es administrativa y es la única superficie que esta feature expone. El catálogo público NO se ve afectado.

## Technical Context

**Language/Version**: C# sobre .NET 10 (ASP.NET 10); TypeScript sobre Angular 22.

**Primary Dependencies**: Entity Framework Core con el proveedor Npgsql (complementaria: necesaria para usar PostgreSQL desde EF Core); Bootstrap 5, CoreUI for Angular Free, Bootswatch Minty y Font Awesome Free para la interfaz administrativa. No se incorporan otras librerías de dominio o validación.

**Storage**: PostgreSQL 17 en el contenedor Docker `psql-17`. El esquema se gestiona con migraciones de EF Core.

**Testing**: xUnit para el backend y el ejecutor de pruebas por defecto del proyecto Angular para el frontend (complementarias, justificadas en research.md R-8). Las pruebas de integración usan una base de datos dedicada dentro de la instancia `psql-17`.

**Target Platform**: Entorno de desarrollo local. Producción y despliegue quedan fuera de esta feature.

**Project Type**: Aplicación web con backend (API) y frontend administrativo.

**Performance Goals**: No definidos por los requerimientos; no se fijan.

**Constraints**: Arquitectura hexagonal en el backend; sin acoplamiento con el catálogo público; sin conceptos de inventario, recetas ni pedidos.

**Scale/Scope**: No definidos por los requerimientos; no se fijan.

## Constitution Check

*GATE: Debe pasar antes de Phase 0. Se re-evalúa tras Phase 1.*

| Principio | Aplica | Resultado | Evidencia |
|-----------|--------|-----------|-----------|
| I. Desarrollo dirigido por especificaciones | Sí | Cumple | Cada entidad, regla y endpoint traza a un FR de spec.md (tablas de trazabilidad en data-model.md y contracts). Los criterios de aceptación permanecen en spec.md; no se duplican. Lo no definido queda en "Cuestiones abiertas" de research.md, sin inventarlo. |
| II. Disciplina de alcance | Sí | Cumple | Ninguna entidad ni endpoint pertenece a la lista fuera de alcance (sin clientes, sucursales, pedidos, recetas ni inventario). |
| III. Arquitectura hexagonal | Sí (nuevo backend) | Cumple | Proyectos Domain, Application, Infrastructure y Api con dependencias hacia el núcleo (ver Project Structure y research.md R-1). La comprobación de referencias de opciones es un puerto de la capa de aplicación (R-13). EF Core y Npgsql solo en Infrastructure. Autenticación/JWT no forma parte de esta feature. |
| IV. Stack tecnológico | Sí | Cumple (con excepción transitoria autorizada) | Stack obligatorio intacto. Complementarias declaradas y justificadas (Npgsql, xUnit, ejecutor de pruebas de Angular). PostgreSQL 17 usado mediante `psql-17`. La ausencia de autenticación se ampara en la excepción transitoria del Principio IV (constitución v1.2.0): rutas agrupadas bajo `/api/admin/catalog`, sin despliegue fuera de desarrollo y cierre con la feature de usuarios y roles (ver Complexity Tracking). |
| V. Separación de frontends | Sí (API y frontend admin) | Cumple | Se crea solo el frontend administrativo. El catálogo público no se toca y esta feature no expone ningún endpoint público ni comparte estado con él. Si el catálogo público consumirá esta misma API u otra queda para su propia feature. |
| VI. POS único canal de pedidos | No aplicable | — | La feature no crea ni modifica pedidos. |
| VII. Reglas operativas | No aplicable | — | No trata mesas, barra, estados ni cobros. |
| VIII. Extensibilidad de catálogo e inventario | Sí (modelo de productos y variantes) | Cumple | Escenario documentado en data-model.md, sección "Escenario de extensibilidad": nueva característica de variación, categoría de tragos y un producto de cada modalidad de inventario sin modificar el modelo. La marca de disponibilidad es manual y no incorpora conceptos de inventario. |
| IX. Modelo de datos desde los requerimientos | Sí (crea modelo de datos) | Cumple | data-model.md indica el requerimiento de origen de cada entidad; no se usa el esquema SQL de referencia. |

Re-evaluación posterior a Phase 1: sin cambios; todos los principios aplicables siguen cumpliéndose.

## Project Structure

### Documentation (this feature)

```text
specs/001-catalogo-productos/
├── plan.md              # Este archivo (/speckit-plan)
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   └── catalog-admin-api.md   # Phase 1
└── tasks.md             # Phase 2 (/speckit-tasks; no lo crea /speckit-plan)
```

### Source Code (repository root)

```text
backend/
├── BuenaOndaSK.sln
├── src/
│   ├── BuenaOnda.Domain/            # Agregados y reglas del catálogo; sin dependencias externas
│   │   └── Catalog/
│   ├── BuenaOnda.Application/       # Casos de uso y puertos (repositorios, unidad de trabajo)
│   │   └── Catalog/
│   ├── BuenaOnda.Infrastructure/    # EF Core + Npgsql, mapeos, repositorios, migraciones
│   │   └── Persistence/
│   └── BuenaOnda.Api/               # Adaptador de entrada HTTP: controladores y DTOs
│       └── Catalog/
└── tests/
    ├── BuenaOnda.Domain.Tests/
    ├── BuenaOnda.Application.Tests/
    └── BuenaOnda.Api.IntegrationTests/

frontend/
└── admin/                           # Aplicación administrativa/POS (Angular 22)
    └── src/app/
        └── catalog/                 # Módulo de catálogo: páginas, componentes, servicios, modelos
```

**Structure Decision**: solución backend de cuatro proyectos con dirección de dependencias Api → Application → Domain e Infrastructure → Application → Domain; el frontend administrativo se aloja en `frontend/admin`, dejando `frontend/` preparado para una segunda aplicación independiente (catálogo público) sin crearla ahora.

## Complexity Tracking

| Violación / Riesgo | Por qué se acepta | Alternativa más simple descartada |
|--------------------|-------------------|-----------------------------------|
| Excepción transitoria del Principio IV: los endpoints administrativos se implementan sin autenticación ni autorización | Autorizada por la constitución v1.2.0 hasta que se implemente la feature de usuarios y roles. Los requerimientos y la spec excluyen autenticación y roles; implementarla aquí decidiría roles y permisos por suposición | Autenticación provisional: descartada por introducir decisiones reservadas a usuarios y roles. Condiciones: (a) rutas agrupadas bajo `/api/admin/catalog`; (b) sin despliegue ni exposición fuera del entorno de desarrollo; (c) cierre al implementarse usuarios y roles, antes de cualquier entorno accesible por terceros |
