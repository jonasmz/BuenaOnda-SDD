<!--
Sync Impact Report
- Versión: 1.1.0 → 1.2.0 (MINOR). Motivo: se agrega una excepción transitoria y acotada al
  Principio IV (guía materialmente ampliada según la política de versionado); no se elimina ni
  redefine ningún principio.
- Principios agregados o eliminados: ninguno; numeración I a IX intacta.
- Principio con cambio normativo: IV (excepción transitoria a la exigencia de Identity, JWT y
  autorización en funcionalidades administrativas hasta que exista la implementación de usuarios
  y roles, con condiciones de cierre y registro en el plan).
- Sin cambios: alcance funcional, stack tecnológico, demás principios, gobernanza, jerarquía de
  fuentes y esquema de versionado. No se definen roles ni permisos ni se incorporan tecnologías.
- Origen: `/speckit.analyze` de la feature 001 (hallazgo D1) y decisión expresa del responsable del
  proyecto de tratar la falta de autenticación mediante enmienda formal.
- Plantillas: sin cambios requeridos.
- TODOs diferidos: ninguno.
-->

# Constitución de BuenaOndaSK

Esta constitución es la autoridad permanente que gobierna las fases de desarrollo dirigido por
especificaciones de GitHub Spec Kit (`/speckit.specify`, `/speckit.clarify`, `/speckit.plan`,
`/speckit.tasks`, `/speckit.checklist`, `/speckit.analyze` y `/speckit.implement`). Define solo
principios, restricciones y estándares estables; no repite los requerimientos funcionales.

Documentos de referencia:

- Requerimientos funcionales: `../docs/system_requirements.txt` (fuente de verdad sobre las
  capacidades concretas del sistema).
- Esquema SQL de referencia: `../docs/sql_schema.sql` (solo referencia conceptual; ver Principio IX).

## Convenciones normativas

- **DEBE / NO DEBE**: obligación / prohibición.
- **PUEDE**: acción opcional.
- Ninguna otra forma verbal expresa una regla en este documento.
- Toda regla se formula de modo que pueda comprobarse contra un artefacto concreto (spec, plan,
  tareas o código). Si un criterio cuantitativo no está definido en los requerimientos, la
  constitución no lo fija.

## Principios fundamentales

### I. Desarrollo dirigido por especificaciones y prohibición de suposiciones

**Regla**:

- Toda funcionalidad, regla de negocio, concepto de dominio o decisión técnica significativa DEBE
  poder justificarse en: los requerimientos, una especificación aprobada, el plan técnico
  correspondiente o esta constitución.
- NO DEBEN agregarse funcionalidades por conveniencia, anticipación o suposición.
- Una decisión no definida NO DEBE inventarse durante la implementación; DEBE resolverse en la
  especificación o en la planificación.
- Los ejemplos de los requerimientos describen la operación actual y NO DEBEN convertirse
  automáticamente en restricciones rígidas del modelo.
- Cada requisito de una especificación DEBE tener criterios de aceptación verificables, sin fijar
  métricas numéricas que los requerimientos no definan.
- La especificación DEBE contener los criterios de aceptación verificables. El plan técnico DEBE
  mantener trazabilidad hacia los requisitos y criterios de aceptación que implementa, sin
  necesidad de duplicar su contenido.

**Justificación**: evita que la implementación decida por su cuenta lo que el negocio no definió.

**Verificación**: `/speckit.analyze` DEBE rastrear cada requisito, entidad y tarea hasta una fuente
citada; lo que no tenga fuente se reporta como hallazgo. `/speckit.analyze` DEBE comprobar
también la trazabilidad del plan y de las tareas hacia los criterios de aceptación de la
especificación.

### II. Disciplina estricta de alcance

**Regla**:

- El sistema DEBE implementar solo las capacidades del alcance vigente definido en los
  requerimientos.
- Lista canónica de lo fuera de alcance (ninguna otra sección la duplica; solo la referencia):
  múltiples sucursales, gestión de cocina, gestión de chefs, gestión de clientes, gestión de
  empleados y delivery de comidas.
- Toda capacidad ausente de los requerimientos (por ejemplo reservas, turnos o estaciones de cocina
  del esquema SQL de referencia) está fuera de alcance por defecto.
- La referencia a "cliente" dentro del contexto operativo de barra NO DEBE interpretarse como un
  requisito de gestión de clientes ni obliga a introducir una entidad de gestión `Cliente`. La
  representación funcional y técnica mediante la cual se individualizan los consumos de barra DEBE
  definirse en la especificación funcional y en el plan correspondiente.
- Ninguna especificación, modelo de datos, API, componente frontend o implementación DEBE incorporar
  capacidades fuera de alcance sin modificar antes los requerimientos y esta constitución.
- Las especificaciones y planes NO DEBEN introducir complejidad destinada exclusivamente a
  funcionalidades fuera del alcance vigente. Esto NO impide la extensibilidad que los requerimientos
  exijan expresamente (Principio VIII).

**Justificación**: el alcance actual es acotado y el esquema SQL de referencia contiene conceptos que
no lo integran.

**Verificación**: `/speckit.plan` DEBE contrastar entidades, endpoints y pantallas con la lista
canónica; `/speckit.analyze` DEBE reportar toda coincidencia.

### III. Arquitectura hexagonal en el backend

**Regla**:

- El backend DEBE usar arquitectura hexagonal. Esta regla aplica al backend; la arquitectura del
  frontend DEBE definirse en el plan.
- Dominio y casos de uso NO DEBEN depender de infraestructura, persistencia, frameworks externos ni
  mecanismos de presentación. Las dependencias DEBEN apuntar hacia el núcleo.
- Persistencia, identidad y demás servicios externos DEBEN integrarse mediante puertos y
  adaptadores. Entity Framework Core, ASP.NET Identity, JWT y cualquier otro detalle tecnológico
  externo DEBEN permanecer fuera del dominio y de los casos de uso. Su ubicación concreta dentro de
  los adaptadores, proyectos o capas externas DEBE definirse y justificarse en el plan técnico
  respetando la dirección de dependencias de la arquitectura hexagonal.
- Todo plan DEBE justificar cualquier decisión que afecte los límites dominio / aplicación /
  infraestructura.

**Justificación**: los requerimientos establecen la arquitectura hexagonal como parte del stack.

**Verificación**: cuando una feature introduzca o modifique componentes del backend, `/speckit.plan`
DEBE demostrar que la solución respeta las fronteras y la dirección de dependencias de la
arquitectura hexagonal, sin documentar de nuevo la arquitectura completa; `/speckit.analyze` DEBE
reportar toda referencia del dominio o de los casos de uso a tipos de EF Core, Identity o JWT.

### IV. Stack tecnológico e identidad establecidos

**Regla**:

- El siguiente stack base es obligatorio:
  - Backend: ASP.NET 10, Entity Framework Core, ASP.NET Identity, API con autenticación JWT.
  - Persistencia: PostgreSQL 17.
  - Frontend: Angular 22, Bootstrap 5, CoreUI for Angular Free, Bootswatch Minty, Font Awesome Free.
- NO DEBEN sustituirse estas tecnologías por alternativas sin modificar formalmente los
  requerimientos y esta constitución.
- La lista anterior NO es el conjunto exhaustivo de paquetes permitidos. PUEDEN incorporarse
  librerías, paquetes y herramientas complementarias cuando sean necesarias para implementar una
  capacidad especificada, sean compatibles con el stack establecido, no sustituyan una tecnología
  obligatoria y su incorporación esté justificada en el plan técnico correspondiente.
- La instancia PostgreSQL 17 utilizada por el entorno de desarrollo DEBE ejecutarse mediante el
  contenedor Docker denominado `psql-17`.
- Las funcionalidades administrativas DEBEN usar ASP.NET Identity, JWT y autorización basada en
  usuarios y roles. Los roles, permisos y la matriz de autorización NO se definen aquí; se definen en
  la especificación correspondiente.
- Excepción transitoria: hasta que la funcionalidad de usuarios y roles esté implementada, las
  funcionalidades administrativas PUEDEN implementarse sin autenticación ni autorización, siempre
  que: (a) sus rutas administrativas queden agrupadas de modo que la protección pueda aplicarse de
  una sola vez; (b) NO se desplieguen ni se expongan fuera del entorno de desarrollo; (c) el plan de
  cada feature registre esta excepción y su cierre. La excepción DEBE cerrarse con la implementación
  de usuarios y roles y NO PUEDE mantenerse en ningún entorno accesible por terceros.
- La gestión de usuarios de acceso NO DEBE convertirse en gestión de empleados.
- Las incompatibilidades entre versiones o librerías del stack DEBEN resolverse en el plan, sin
  cambiar el stack.

**Justificación**: el stack forma parte de los requerimientos vigentes.

**Verificación**: `/speckit.plan` DEBE declarar y justificar las dependencias complementarias que incorpore;
`/speckit.analyze` DEBE reportar sustituciones del stack obligatorio, tecnologías incompatibles
con él y dependencias que introduzcan decisiones arquitectónicas contrarias a la constitución. NO
DEBE reportar como violación una dependencia auxiliar solo por no figurar en la lista.

### V. Separación entre frontend administrativo/POS y catálogo público

**Regla**:

- DEBEN existir dos aplicaciones frontend independientes: administrativa/POS y catálogo público
  accesible por QR.
- Ambas DEBEN usar el stack visual definido y mantener coherencia de identidad gráfica; la
  reutilización de tecnologías o patrones visuales NO DEBE generar acoplamiento funcional.
- NO DEBEN compartir: sesión de usuario, carrito, estado operativo de pedidos, flujo operativo,
  permisos ni funcionalidades administrativas.
- El catálogo público NO DEBE exponer rutas, componentes, permisos ni funcionalidades
  administrativas o del POS.
- La información accesible desde el catálogo público DEBE limitarse a información necesaria para la
  consulta, presentación y funcionamiento de las capacidades públicas especificadas. Puede incluir,
  entre otros datos públicos definidos por la correspondiente especificación, producto,
  disponibilidad, categoría, descripción, imagen y precio.
- El catálogo público NO DEBE acceder a información administrativa, permisos, estado operativo
  interno no destinado al catálogo, operaciones administrativas ni capacidades del POS.
- Las operaciones accesibles desde el catálogo público NO DEBEN modificar datos administrativos u
  operativos ni crear o modificar pedidos reales; respecto de ellos solo PUEDEN ser de consulta.
- El plan DEBE definir si ambos frontends consumen una misma API o APIs distintas, cómo se organizan
  los endpoints y cómo se implementan técnicamente estos límites.

**Justificación**: los requerimientos separan ambas aplicaciones funcional y operativamente.

**Verificación**: cuando una feature afecte al frontend administrativo/POS, al catálogo público, a la API consumida por
alguno de ellos o a información compartida entre ambos, `/speckit.plan` DEBE identificar los
artefactos de cada aplicación y los datos compartidos; `/speckit.analyze` DEBE reportar cualquier módulo, estado o endpoint de escritura
compartido entre ellas.

### VI. El POS es el único canal de creación de pedidos reales

**Regla**:

- Los pedidos operativos DEBEN originarse exclusivamente en el POS administrativo.
- El catálogo público y su carrito solo sirven para consulta, simulación, composición tentativa y
  estimación de costos.
- El carrito público NO DEBE crear pedidos, enviarlos al POS, modificar pedidos existentes, iniciar
  procesos operativos ni sustituir la toma de pedidos del operador.
- Esta regla NO DEBE romperse sin modificar antes los requerimientos y esta constitución.

**Justificación**: los requerimientos definen al POS como único canal operativo de pedidos.

**Verificación**: `/speckit.plan` y `/speckit.analyze` DEBEN comprobar que ningún flujo, endpoint o
tarea del catálogo público crea o modifica pedidos.

### VII. Fidelidad a las reglas operativas del negocio

**Regla**:

- Mesas y barra DEBEN tratarse como contextos operativos diferentes.
- El consumo de una mesa se cobra por el consumo completo de la mesa; en la barra, por cliente
  (sin decidir aún su representación; ver Principio II).
- La cantidad actual de mesas NO DEBE codificarse como límite estructural fijo.
- El operador DEBE poder establecer y modificar estados de pedidos. Los estados concretos y sus
  transiciones NO DEBEN inventarse hasta definirse en su especificación funcional.
- Las reglas de dominio no establecidas NO DEBEN decidirse fuera de la especificación funcional o
  del plan correspondiente.

**Justificación**: son reglas operativas ya definidas en los requerimientos.

**Verificación**: `/speckit.plan` DEBE mostrar que el modelo distingue ambos contextos y que la
cantidad de mesas es un dato y no una estructura; `/speckit.analyze` DEBE reportar estados o
transiciones sin especificación.

### VIII. Modelo de catálogo e inventario extensible y no rígido

**Regla**:

- El modelo de productos DEBE representar distintas clases de variación (tamaños, presentaciones,
  sabores, variedades y otras definibles después) sin estructuras específicas por producto derivadas
  de los ejemplos actuales.
- Incorporar nuevos productos o variantes NO DEBE requerir rediseñar el catálogo; el plan DEBE
  demostrarlo. La futura incorporación de tragos DEBE ser posible sin rediseñar el modelo de
  productos, pero NO DEBEN implementarse funcionalidades específicas de tragos hasta que se
  requieran.
- El inventario DEBE contemplar dos modalidades conceptuales: productos elaborados (asociables a
  receta, con ingredientes y cantidades cuyo consumo pueda derivarse de la elaboración o venta) y
  productos de stock directo (controlados por existencias, sin receta necesaria).
- El modelo NO DEBE forzar recetas para todos los productos ni tratar todos como stock directo.
- Unidades de medida, movimientos, ajustes, estructura definitiva de recetas y momento del descuento
  de stock DEBEN resolverse en especificaciones y plan.

**Justificación**: los requerimientos piden extensibilidad explícita y dos modalidades de inventario.

**Verificación**: cuando el plan diseñe o modifique el modelo de productos, variantes o inventario,
`/speckit.plan` DEBE incluir un escenario documentado que agregue un nuevo tipo de
variante y un producto de cada modalidad de inventario sin modificar el modelo existente.

### IX. El modelo de datos surge de los requerimientos, no del esquema SQL de referencia

**Regla**:

- El esquema SQL es solo referencia conceptual (útil para las relaciones entre productos, recetas,
  ingredientes, cantidades y stock). NO DEBE tratarse como contrato, modelo definitivo, fuente
  superior a los requerimientos ni conjunto obligatorio de tablas.
- Cada entidad, relación o concepto tomado de él DEBE justificarse contra los requerimientos y las
  especificaciones.
- NO DEBEN trasladarse conceptos fuera de alcance (lista canónica del Principio II).

**Justificación**: los requerimientos declaran que el esquema es orientativo y no prevalece sobre
ellos.

**Verificación**: cuando un plan cree o modifique el modelo de datos, su `data-model.md` DEBE indicar, para cada entidad, el
requerimiento que la origina; `/speckit.analyze` DEBE reportar entidades sin origen.

## Restricciones tecnológicas y arquitectónicas

Las restricciones vigentes se establecen en los Principios III (arquitectura hexagonal del backend),
IV (stack e identidad) y V (separación de frontends); esta sección no las repite.

La regla sobre el contenedor `psql-17` se establece en el Principio IV.

## Reglas de desarrollo dirigido por especificaciones y verificación constitucional

- Toda especificación, plan y conjunto de tareas DEBE poder demostrar compatibilidad con esta
  constitución.
- Durante `/speckit.plan` y `/speckit.analyze` se DEBEN detectar explícitamente:
  - funcionalidades fuera de alcance;
  - sustituciones no autorizadas del stack obligatorio;
  - violaciones de la arquitectura hexagonal;
  - acoplamiento entre los dos frontends;
  - creación de pedidos desde el catálogo público;
  - modelados rígidos basados solo en ejemplos;
  - incorporación automática de conceptos del esquema SQL;
  - decisiones inventadas que los requerimientos reservaron para etapas posteriores.
- Las verificaciones de cada principio DEBEN aplicarse cuando el principio sea relevante para la
  feature o cambio analizado.
- Los Constitution Checks DEBEN evaluar todos los principios y marcar como "no aplicable", con una
  breve justificación, aquellos que legítimamente no resulten afectados por la feature. Los
  principios aplicables DEBEN indicar cumple / no cumple y la evidencia correspondiente.
- Una implementación que contradiga un principio NO DEBE considerarse válida aunque funcione
  técnicamente.

## Gobernanza

**Jerarquía de fuentes**: ante un conflicto prevalece, en este orden: 1) constitución,
2) requerimientos funcionales, 3) especificación aprobada, 4) plan técnico, 5) tareas e
implementación. La constitución limita cómo se puede ampliar o interpretar lo demás; los
requerimientos siguen siendo la fuente de verdad sobre las capacidades concretas del sistema.

**Reglas**:

- La constitución prevalece sobre decisiones ad hoc de planes, tareas o implementaciones.
- Una especificación NO PUEDE modificar silenciosamente un principio.
- Las excepciones DEBEN ser explícitas, justificadas y reflejadas en una modificación formal cuando
  afecten un principio no negociable.
- Toda modificación DEBE evaluarse por su impacto sobre especificaciones, planes, tareas e
  implementaciones existentes.
- La constitución DEBE evolucionar con poca frecuencia; lo que aún no sea estable DEBE permanecer
  en requerimientos, especificaciones o planes.
- Toda revisión del proyecto DEBE verificar el cumplimiento constitucional.

**Versionado** (semántico simple): MAJOR = elimina o redefine principios fundamentales;
MINOR = agrega principios o restricciones; PATCH = aclaraciones de redacción sin cambio normativo.

**Versión**: 1.2.0 | **Ratificada**: 2026-09-21 | **Última modificación**: 2026-09-21
