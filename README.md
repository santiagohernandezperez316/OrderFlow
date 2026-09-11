# OrderFlow

Prueba técnica OrderFlow: una tienda recibe pedidos, cada pedido reserva inventario antes de
confirmarse. Full-stack: backend en .NET 8 (Clean Architecture + CQRS/MediatR) y frontend en
React 19 + TypeScript (arquitectura hexagonal del lado cliente) que consume la API REST y se
suscribe a SignalR para reflejar cambios de estado en tiempo real.

## Estructura del repositorio

```
.
├── backend/            # .NET 8 — un proyecto por carpeta (ver "Capas del backend")
│   ├── OrderFlow.Domain/
│   ├── OrderFlow.Application/
│   ├── OrderFlow.Infrastructure/
│   ├── OrderFlow.Contracts/
│   ├── OrderFlow.Api/            (+ Dockerfile propio)
│   ├── OrderFlow.InventoryWorker/ (+ Dockerfile propio)
│   └── OrderFlow.*.Tests/        # un proyecto de test por capa
├── frontend/            # React 19 + TypeScript (ver "Frontend"), + Dockerfile propio
├── k8s/                 # manifiestos de Kubernetes (bonus, ver "Kubernetes")
├── docker-compose.yml    # levanta los 5 servicios con un solo comando
└── README.md             # este archivo — única fuente de verdad para todo el proyecto
```

## Servicios

- **OrderFlow.Api**: expone `POST /orders`, `GET /orders`, `GET /orders/{id}`. Al crear un
  pedido lo persiste en estado `Pending` y publica `OrderCreated` vía el outbox transaccional
  de MassTransit (EF Outbox). También consume `StockReserved`/`StockRejected` para mover el
  pedido a `Confirmed`/`Rejected`.
- **OrderFlow.InventoryWorker**: consume `OrderCreated`, descuenta stock de forma idempotente
  y publica `StockReserved` o `StockRejected`.

## Capas del backend (Clean Architecture)

- **OrderFlow.Domain**: entidades (`Order`, `Product`), value objects y reglas de negocio puras
  (transición de estado, validaciones). Sin ninguna dependencia externa — ni EF Core, ni
  MassTransit, ni ASP.NET — verificado por `OrderFlow.Architecture.Tests`.
- **OrderFlow.Application**: casos de uso (comandos/queries de MediatR) y los puertos que
  necesitan (`IOrdersUnitOfWork`, `IEventPublisher`, `IOrderRealtimeNotifier`, etc.). Depende
  solo de `Domain` y `Contracts`, nunca de una implementación concreta de infraestructura (ver
  `ApplicationLayer_ShouldNotDependOnInfrastructureLibraries` en `OrderFlow.Architecture.Tests`).
- **OrderFlow.Infrastructure**: implementa los puertos de `Application` (EF Core, outbox de
  MassTransit, repositorios) y expone las extensiones de arranque (`AddOrdersPersistence`,
  `MigrateAndSeedOrders`, etc.). Es la única capa que conoce SQL Server y MassTransit como
  tecnologías concretas.
- **OrderFlow.Contracts**: los eventos de integración compartidos entre Orders e Inventory
  (`OrderCreated`, `StockReserved`, `StockRejected`) — el único paquete referenciado por ambos
  servicios para hablar el mismo "idioma" de mensajería sin acoplarse a los tipos internos del
  otro.
- **OrderFlow.Api** / **OrderFlow.InventoryWorker**: los dos puntos de entrada (hosts). Cablean
  la inyección de dependencias, exponen HTTP/SignalR (solo Api) y consumen eventos de
  MassTransit.
- **frontend/** (React + TypeScript): consume la REST API de Orders API (`http://localhost:8080`)
  y se suscribe a `OrdersHub` vía SignalR para reflejar cambios de estado en tiempo real. No
  conoce a Inventory Worker ni a RabbitMQ directamente — es un cliente puro de Orders API. Su
  propia arquitectura interna (hexagonal, del lado cliente) se describe en la sección
  [Frontend](#frontend-react--typescript) más abajo.

## Frontend (React + TypeScript)

`frontend/` es una SPA en React 19 + TypeScript (Vite + Tailwind) organizada como arquitectura
hexagonal del lado cliente, en `frontend/src/features/orders/`:

- **domain/**: tipos puros (`Order`, `CreateOrderInput`, `OrderApiError`/`OrderApiException`) y
  los puertos (`OrderRepositoryPort`, `OrderRealtimePort`) que la aplicación necesita. Sin
  ninguna dependencia de React, `fetch` ni `@microsoft/signalr`.
- **application/**: los hooks de caso de uso (`useOrdersList`, `useCreateOrder`) que reciben los
  puertos por parámetro y exponen estados explícitos (`useCreateOrder`:
  `idle`/`loading`/`success`/`error`; `useOrdersList`: `loading`/`success`/`error`, sin `idle`
  porque la lista siempre carga al montar), nunca una implementación concreta de infraestructura.
- **infrastructure/**: los adaptadores concretos — `httpOrderRepository` (REST contra Orders
  API, distingue error de validación / de negocio / de servidor por `status` + shape del body) y
  `signalrOrdersAdapter` (suscripción a `OrdersHub`, expone el estado real de la conexión:
  `connecting`/`connected`/`reconnecting`/`disconnected`).
- **ui/**: componentes presentacionales puros (`OrderForm`, `OrdersList`, `StatusPill`,
  `LiveIndicator`, etc.) — reciben datos y callbacks por props, nunca llaman `fetch` ni el hub.

`frontend/src/app/container.ts` es el único lugar donde se instancian los adaptadores concretos
e inyectan en los hooks — el resto de la app solo conoce los puertos. Cuando `OrdersHub` no está
`Connected`, `useOrdersList` cae a *polling* cada 10s como respaldo (`LiveIndicator` refleja el
modo real, no un valor fijo).

Tests del frontend: Vitest + Testing Library, mockeando `OrderRepositoryPort`/`OrderRealtimePort`
sin tocar `ui/` — 7 tests entre `useOrdersList.test.ts` y `useCreateOrder.test.ts` (estados de
creación/listado, clasificación de errores de validación vs. negocio, y que el polling se activa
solo cuando el hub no está `Connected`).

```bash
cd frontend
npm install
npm run test    # vitest run
npm run build   # tsc -b && vite build
```

### Responsive (mobile)

`index.html` incluye `<meta name="viewport" content="width=device-width, initial-scale=1.0">` y
el layout usa Tailwind con dos breakpoints reales:

- **`App.tsx`**: en mobile (`< md`, 768px) el formulario y la lista de pedidos se apilan en una
  sola columna a lo ancho del viewport (`flex-col`), con padding reducido (`px-4 py-6`). A partir
  de `md:` pasa a dos columnas, con el formulario como sidebar fijo (`md:sticky md:w-[380px]`) y
  padding mayor (`sm:px-6 sm:py-10`).
- **Inputs (`Input`/`Select`)**: usan `text-base` (16px) en mobile y `sm:text-sm` (14px) desde
  `sm:` — por debajo de 16px, Safari en iOS hace zoom automático al enfocar un campo, así que se
  evita ese salto.
- **Tabla de pedidos (`OrdersList`)**: tiene 6 columnas, demasiadas para una pantalla angosta sin
  romper el layout. Va envuelta en un contenedor `overflow-x-auto` con `min-w-[560px]` en la
  tabla, así que en mobile se puede hacer *scroll* horizontal en vez de comprimir o desbordar las
  columnas.

## Decisiones de arquitectura

- **Bounded contexts separados por schema**: Orders (`orders.*`) e Inventory (`inventory.*`)
  viven en la misma base de datos física (por simplicidad de despliegue en esta prueba) pero
  en schemas distintos, con sus propios `DbContext`, migraciones y tabla de historial de
  migraciones (`__EFMigrationsHistory` por schema). Ninguno de los dos servicios referencia
  las tablas del otro directamente.
- **Catálogo de SKU duplicado a propósito**: Orders valida el SKU contra su propia tabla
  `orders.KnownSkus` (no contra `inventory.Products`). Es la forma correcta de mantener los
  contextos desacoplados, al costo de mantener el catálogo de SKUs sincronizado en dos sitios
  (ver trade-offs).
- **Outbox transaccional (MassTransit + EF Outbox)** en ambos servicios: el evento se guarda
  en la misma transacción que el cambio de estado de negocio, así que nunca hay pérdida de
  eventos si el broker está caído en el momento de publicar (detalle abajo).
- **Idempotencia del worker vía tabla de eventos procesados**: `Inventory.ProcessedEvents`
  guarda el `EventId` (MessageId de MassTransit) de cada `OrderCreated` ya procesado, dentro
  de la misma transacción que el descuento de stock. Si el mensaje llega duplicado (redelivery,
  reintento del broker), el handler no vuelve a descontar ni a republicar.
- **Transición de estado idempotente en el pedido**: `Order.Confirm()`/`Reject()` solo aplican
  si el pedido sigue en `Pending`. Si `StockReserved`/`StockRejected` llega duplicado, la
  segunda vez es un no-op.
- **Health checks separados**: `/health/live` (sin dependencias, para saber si el proceso está
  vivo) y `/health/ready` (chequea la base de datos, para saber si puede recibir tráfico).

## Trade-offs

- El catálogo de SKUs existe por duplicado (`orders.KnownSkus` e `inventory.Products`). Es el
  precio de no acoplar los bounded contexts a nivel de base de datos. Con más servicios en
  juego, esto se resolvería con un evento `ProductRegistered`/`ProductDiscontinued` publicado
  por Inventory y consumido por Orders para mantener su catálogo sincronizado.
- Orders e Inventory comparten una sola instancia de SQL Server (una base de datos, dos
  schemas) para simplificar el `docker compose up` de la prueba. En producción real cada
  bounded context tendría su propia base de datos físicamente separada.
- No hay reintentos/backoff configurados explícitamente a nivel de consumer de MassTransit
  más allá de los defaults del framework; ver "qué haría distinto con más tiempo".

## Cómo correr todo

```bash
cp .env.example .env   # ajustar contraseñas si hace falta
docker compose up --build
```

Esto levanta los 5 servicios del `docker-compose.yml`: SQL Server, RabbitMQ, Orders API,
Inventory Worker y el frontend (`orders-frontend`, servido por nginx). Ambos servicios de
backend aplican sus migraciones EF Core y siembran datos (3 productos y sus SKUs
correspondientes) automáticamente al arrancar, sin pasos manuales. Orders API queda expuesta en
`http://localhost:8080`, el frontend en `http://localhost:8082` (ya con el bundle apuntando a
Orders API vía `VITE_API_BASE_URL`/`VITE_HUB_URL`, inyectadas en build time — ver
`frontend/Dockerfile`). RabbitMQ Management en `http://localhost:15672`.

Dockerfile multi-stage en los tres servicios: `frontend/Dockerfile` compila el bundle con Node y
lo sirve con nginx; `backend/OrderFlow.Api/Dockerfile` y `backend/OrderFlow.InventoryWorker/Dockerfile`
compilan (`build`) con `mcr.microsoft.com/dotnet/sdk:8.0` y ejecutan (`final`) sobre
`mcr.microsoft.com/dotnet/aspnet:8.0` — la imagen final no incluye el SDK.

Swagger UI queda disponible en `http://localhost:8080/swagger` (redirige a
`/swagger/index.html`; el documento OpenAPI crudo está en `/swagger/v1/swagger.json`) para que
cualquier evaluador pruebe la API sin `curl`. El compose fija `ASPNETCORE_ENVIRONMENT=Development`
para `orders-api` únicamente con ese fin — no es información sensible y en este proyecto no
habilita nada más (no hay `UseDeveloperExceptionPage` ni configuración adicional gateada por
entorno).

Toda la configuración (cadenas de conexión, credenciales de RabbitMQ, entorno) se inyecta por
variables de entorno — nada hardcodeado en el código ni en `appsettings.json`.

## Qué pasa si Inventory no responde

`OrderCreated` queda en la tabla de outbox de Orders hasta que MassTransit logra entregarlo al
broker; una vez en RabbitMQ, el mensaje espera en la cola hasta que Inventory Worker vuelva a
estar disponible para consumirlo — no se pierde. Mientras tanto, el pedido permanece en
`Pending` en la API (así lo ve el cliente vía `GET /orders/{id}`). Cuando Inventory Worker
vuelve, procesa el backlog de mensajes pendientes con la misma garantía de idempotencia
(`ProcessedEvents`), así que no importa si el mensaje fue entregado más de una vez.

## Qué pasa si el broker está caído

Gracias al outbox transaccional, `OrderCreated` se persiste en la tabla de outbox **dentro de
la misma transacción de base de datos** que crea el pedido. Si RabbitMQ está caído en ese
instante, la creación del pedido no falla ni se pierde el evento: el `OutboxDeliveryService`
de MassTransit reintenta la publicación en segundo plano hasta que el broker vuelve a estar
disponible. Lo mismo aplica en Inventory Worker al publicar `StockReserved`/`StockRejected`.

El otro escenario es que RabbitMQ esté caído cuando Orders API o Inventory Worker **arrancan**
e intentan conectarse como consumers. MassTransit no falla el arranque del host en ese caso: el
bus queda reintentando la conexión en segundo plano (backoff por defecto del framework), y los
consumers (`StockReservedConsumer`, `StockRejectedConsumer`, `OrderCreatedConsumer`) simplemente
no reciben mensajes hasta que la conexión se restablece. La API sigue respondiendo a
`POST`/`GET /orders` con normalidad durante ese tiempo — servir HTTP no depende de RabbitMQ —,
solo se retrasa el procesamiento asíncrono del pedido hasta que el broker vuelve y los consumers
se reconectan.

## Qué haría distinto con más tiempo

- Sincronizar el catálogo de SKUs entre Orders e Inventory vía eventos de dominio en lugar de
  seeds duplicados en cada servicio.
- Agregar políticas explícitas de reintento y dead-lettering en los consumers de MassTransit
  (`UseMessageRetry`, `UseInMemoryOutbox` en el consumer, cola de dead-letter con alertas).
- Tests de integración reales contra SQL Server/RabbitMQ en contenedores (Testcontainers) además
  de los tests unitarios actuales, para cubrir el flujo completo Orders → RabbitMQ → Inventory →
  RabbitMQ → Orders.
- Exponer métricas (Prometheus) y trazas distribuidas (OpenTelemetry) para poder observar la
  latencia entre `OrderCreated` y la confirmación/rechazo del pedido.
- Paginación en `GET /orders`.

## SignalR (implementado)

Cada cambio de estado de un pedido levanta la notificación de MediatR
`OrderStatusChangedNotification` (`OrderFlow.Application/Orders/Notification`), que ahora tiene
dos handlers: el de logging existente (`OrderStatusChangedLogHandler`) y uno nuevo
(`OrderStatusChangedHubHandler`) que reenvía el cambio a un `Hub` de SignalR. Ninguno de los
command handlers existentes (`ConfirmOrderHandler`/`RejectOrderHandler`) se modificó.

Se mantiene la separación de capas: `OrderFlow.Application` no depende de SignalR, solo del
puerto `IOrderRealtimeNotifier` (`OrderFlow.Application/Ports`). `OrderFlow.Api` (capa externa)
implementa ese puerto con `SignalROrderRealtimeNotifier` y expone `OrdersHub` en
`/hubs/orders`, mapeado en `Program.cs`.

Payload que emite el hub al evento `"OrderStatusChanged"`: `(orderId: Guid, status: string, reason:
string | null)`, con `status` en `"Pending" | "Confirmed" | "Rejected"` y `reason` con el motivo del
rechazo (por ejemplo `"stock insuficiente para SKU-001"`) solo cuando `status` es `"Rejected"` — es
`null` en `"Confirmed"`. El motivo se origina en `ReserveStockHandler` (Inventory Worker), viaja en
`StockRejected.Reason`, se persiste en `Order.RejectionReason` (columna nullable en `orders.Orders`)
y se expone también en `OrderDto.RejectionReason` vía `GET /orders`/`GET /orders/{id}`.

En el frontend, `App.tsx` recuerda el `orderId` del último pedido creado en la sesión actual; si
ese pedido específico pasa a `Rejected`, muestra un toast explícito (`shared/ui/Toast.tsx`) con el
motivo real ("Rechazado: stock insuficiente para SKU-001"), además del cambio normal de
`StatusPill` + flash de fondo en la fila de la tabla que ya aplica a cualquier pedido visible en
`OrdersList` (el suyo o el de otra sesión).

## CORS

Orders API restringe CORS a un único origen explícito (sin wildcard), configurado por la
variable de entorno `FRONTEND_ORIGIN` (ver `.env.example`), con `AllowCredentials()` habilitado
porque el negotiate inicial de SignalR lo requiere. En un despliegue real (ver `k8s/`) esto se
resolvería normalmente a nivel de Ingress/API Gateway en vez de código explícito en el backend;
para esta prueba, CORS en el backend es la opción más simple y confiable.

## Tests

30 tests en total, repartidos en 4 proyectos:

- `OrderFlow.Domain.Tests` (9): reglas de validación de `Order`/`Product` y transición de estado
  (`Confirm`/`Reject` idempotentes).
- `OrderFlow.Application.Tests` (6): idempotencia de `ReserveStockHandler` a nivel de worker (un
  `EventId` ya procesado no vuelve a descontar stock ni a publicar); mapeo `Order -> OrderDto`; el
  happy path de `RejectOrderHandler` (transiciona a `Rejected`, persiste `RejectionReason` y publica
  `OrderStatusChangedNotification` con el motivo); y el caso borde de evento fuera de orden
  (`RejectOrderHandlerTests` — un `StockRejected` que llega después de que el pedido ya fue
  `Confirmed` no cambia el estado, no setea `RejectionReason` y queda logueado como warning).
- `OrderFlow.Api.Tests` (5): validación de `CreateOrderCommandValidator` (cliente vacío, cantidad
  fuera de rango, caso válido) y un test end-to-end (`CreateOrderEndpointTests`) que levanta la
  API completa con `WebApplicationFactory` y verifica `POST /orders` con payload inválido
  respondiendo `400` real.
- `OrderFlow.Architecture.Tests` (10): reglas de dependencia entre capas (ArchUnitNET), incluida
  la que impide que `Application` dependa de MassTransit o EF Core directamente.

`backend/OrderFlow.sln` agrupa los 10 proyectos (6 de producto + 4 de test), así que basta un
solo comando para correr los 30 tests:

```bash
cd backend
dotnet test
```

Frontend: 7 tests (Vitest + Testing Library) en `frontend/src/features/orders/application/`,
mockeando `OrderRepositoryPort`/`OrderRealtimePort` (ver sección
[Frontend](#frontend-react--typescript)):

```bash
cd frontend
npm run test
```

### Verificación manual de idempotencia (herramienta de diagnóstico, no parte del contrato)

Además del test unitario de `ReserveStockHandler`, Inventory Worker expone un endpoint
**solo para pruebas manuales**, apagado por defecto:
`POST /diagnostics/replay-order-created` (`backend/OrderFlow.InventoryWorker/Diagnostics/ReplayOrderCreatedEndpoint.cs`).
Republica un `OrderCreated` con el `EventId` exacto que le pases, para poder invocarlo dos veces
seguidas con el mismo `EventId` y comprobar en la base de datos que la segunda vez no vuelve a
descontar stock.

Se activa con `ENABLE_DIAGNOSTICS=true` en `.env` (por defecto `false`) y queda expuesto en
`http://localhost:8081` cuando el compose está arriba:

```bash
curl -X POST http://localhost:8081/diagnostics/replay-order-created \
  -H "Content-Type: application/json" \
  -d '{"eventId":"11111111-1111-1111-1111-111111111111","orderId":"22222222-2222-2222-2222-222222222222","sku":"SKU-001","cantidad":1}'

# repetir el mismo curl con el mismo eventId: la segunda vez no debe cambiar el stock
```

Verificar en SSMS/`sqlcmd` contra `OrderFlowDb`:

```sql
SELECT * FROM inventory.ProcessedEvents WHERE EventId = '11111111-1111-1111-1111-111111111111';
SELECT Sku, Stock FROM inventory.Products WHERE Sku = 'SKU-001';
```

Este endpoint no existe si `ENABLE_DIAGNOSTICS` no está en `true` — no queda expuesto por
accidente en un despliegue real.

## Audit de dependencias

Se corrió `dotnet list package --vulnerable --include-transitive` apuntando explícitamente a
`-s https://api.nuget.org/v3/index.json` (el feed offline configurado por defecto en este
entorno no expone información de vulnerabilidades y da falsos negativos si no se fuerza esa
fuente).

**5 paquetes transitivos corregidos**, pineados explícitamente en `backend/OrderFlow.Infrastructure/OrderFlow.Infrastructure.csproj`
(que es donde se originan realmente, vía EF Core/MassTransit/Azure.Identity): `Azure.Identity`
(1.10.3 → 1.21.0), `Microsoft.Extensions.Caching.Memory` (8.0.0 → 10.0.12),
`Microsoft.Identity.Client` (4.56.0 → 4.89.0), `System.Formats.Asn1` (5.0.0 → 10.0.12) y
`System.Text.Json` (8.0.0 → 10.0.12).

**AutoMapper 13.0.0 — riesgo evaluado y aceptado explícitamente, no un olvido.** Tiene una
vulnerabilidad High conocida (`GHSA-rvv3-g6hj-g44x`): DoS por recursión no controlada al mapear
grafos de objetos con miles de niveles de anidación. El fix está en las versiones 15.1.1+/16.1.1+,
pero esas versiones (igual que la 13.x actual) exigen licencia comercial de terceros para uso
comercial. Se decidió mantener 13.0.0 porque el único mapeo del proyecto es `Order -> OrderDto`,
un mapeo plano sin anidación — el vector de ataque (grafos profundamente anidados) no es
alcanzable con el modelo de datos actual, y pagar una licencia comercial para blindarse de un
vector inalcanzable no se justifica para el alcance de esta prueba. En un proyecto de mayor
duración, la recomendación sería remover la dependencia directamente y reemplazar ese único
mapeo por una conversión manual, dado lo trivial que es — más barato que pagar la licencia o
aceptar el riesgo indefinidamente.

## Kubernetes (bonus)

`k8s/` contiene el manifiesto de Orders API: `Deployment` (2 réplicas, límites de recursos,
liveness/readiness sobre `/health/live` y `/health/ready`), `Service` (ClusterIP) y `ConfigMap`
con la configuración no sensible. Las credenciales (cadena de conexión, password de RabbitMQ)
se inyectan desde un `Secret` (`orders-api-secrets`, no incluido — se crea con
`kubectl create secret generic`).

Para desplegar el resto del sistema:

- **RabbitMQ y SQL Server**: como `StatefulSet` con volumen persistente (`PersistentVolumeClaim`)
  cada uno, más un `Service` headless o `ClusterIP` para que Orders API e Inventory Worker los
  resuelvan por nombre DNS interno del cluster.
- **Inventory Worker**: un `Deployment` sin `Service` (no expone HTTP), con las mismas
  variables de entorno que Orders API para RabbitMQ y su propia cadena de conexión a
  `InventoryDb`, y liveness probe basada en un `exec` simple (por ejemplo, verificar que el
  proceso siga vivo) ya que no expone un endpoint HTTP de salud.
- Las migraciones (`Database.Migrate()`) se aplican al arrancar cada servicio; en un cluster
  real se recomendaría moverlas a un `Job` de Kubernetes que corra antes del rollout
  (`initContainer` o *pre-install hook* si se usa Helm), para no correr migraciones
  concurrentes desde múltiples réplicas del mismo `Deployment`.
