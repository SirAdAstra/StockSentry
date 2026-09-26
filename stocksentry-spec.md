# StockSentry — тех. задание

Складской учёт с автоматическими алертами о низком остатке через Kafka. Цель — портфолио-проект под middle .NET разработчика, закрывающий два подтверждённых пробела: ASP.NET Core Web API и EF Core.

## 1. Стек

- **.NET 10**, ASP.NET Core Web API
- **EF Core** + PostgreSQL
- **Kafka** (producer + consumer, отдельный consumer-проект)
- **JWT** авторизация с ролями (Admin / Manager / Viewer)
- **FluentValidation**, **Serilog**
- **xUnit** (unit + integration tests)
- **Docker / docker-compose** (API, Worker, PostgreSQL, Kafka)
- **GitHub Actions** (CI: build + тесты на каждый push)

## 2. Структура решения (VS Code / .sln)

```
StockSentry/
├── src/
│   ├── Api/            # контроллеры, middleware, Program.cs
│   ├── Application/    # DTO, интерфейсы сервисов, валидация
│   ├── Domain/         # сущности, enum'ы, бизнес-правила
│   ├── Infrastructure/ # DbContext, репозитории, Kafka producer
│   └── AlertWorker/    # отдельный консольный воркер — Kafka consumer
├── tests/
│   ├── UnitTests/
│   └── IntegrationTests/
├── docker-compose.yml
└── README.md
```

Отдельный проект `AlertWorker` — намеренное решение: показывает, что ты умеешь проектировать decoupled-сервисы, а не всё пихать в один процесс.

## 3. Доменная модель

- **Category** — Id, Name
- **Supplier** — Id, Name, ContactEmail, Phone
- **Warehouse** — Id, Name, Location
- **Product** — Id, Name, SKU, CategoryId, SupplierId, ReorderThreshold, UnitPrice
- **StockItem** — Id, ProductId, WarehouseId, Quantity *(остаток конкретного товара на конкретном складе)*
- **StockMovement** — Id, StockItemId, ChangeAmount, MovementType (In / Out / Adjustment), Timestamp, Note, PerformedByUserId
- **LowStockAlert** — Id, ProductId, WarehouseId, TriggeredAt, ThresholdAtTrigger, QuantityAtTrigger, Resolved (bool)
- **User** — Id, Username, PasswordHash, Role (Admin / Manager / Viewer)

Связи: Product 1→N StockItem, StockItem 1→N StockMovement, Warehouse 1→N StockItem. Это даёт EF Core реальные one-to-many связи и повод показать `Include`/проекции в запросах.

## 4. Бизнес-логика

1. Любое создание `StockMovement` пересчитывает `StockItem.Quantity`.
2. Если новое значение `Quantity` < `Product.ReorderThreshold` — API публикует событие в Kafka-топик `stock.low-stock-alerts` (payload: ProductId, ProductName, WarehouseId, CurrentQuantity, Threshold, Timestamp).
3. `AlertWorker` слушает топик, на каждое событие пишет запись в `LowStockAlert` и логирует. Это симулирует "уведомление отдела закупок", не завязываясь на реальную отправку email.
4. Ручка `PATCH /api/alerts/{id}/resolve` — закрыть алерт вручную.

## 5. API (основные эндпоинты)

**Auth**
- `POST /api/auth/register`
- `POST /api/auth/login` → JWT

**Products / Categories / Suppliers / Warehouses**
- Стандартный CRUD: `GET` (список с пагинацией и фильтрами), `GET /{id}`, `POST`, `PUT`, `DELETE` — доступ на запись только Admin/Manager

**Stock**
- `GET /api/stock` — текущие остатки, фильтр по складу/товару
- `POST /api/stock/movements` — зафиксировать движение (ключевая бизнес-операция)
- `GET /api/stock/movements` — история, фильтры по дате/товару/складу

**Alerts**
- `GET /api/alerts` — история алертов, фильтр по resolved
- `PATCH /api/alerts/{id}/resolve`

**Служебное**
- `GET /health`
- Swagger на `/swagger`

## 6. Сквозные механизмы

- Глобальный middleware для исключений → единый формат ошибки в JSON
- FluentValidation на все входящие DTO
- Serilog → консоль + файл
- JWT + `[Authorize(Roles = "...")]` на пишущих эндпоинтах
- Конфигурация через `appsettings.json` + переменные окружения, секреты (JWT-ключ, строка подключения) не коммитить — `.env` в `.gitignore`

## 7. Тесты

- **Unit** (xUnit): пересчёт `StockItem.Quantity`, логика триггера алерта, валидаторы
- **Integration**: пару ключевых эндпоинтов через `WebApplicationFactory` + EF Core InMemory/SQLite (без Testcontainers на первой итерации — это ускоряет старт; Testcontainers для Postgres/Kafka можно добавить позже как stretch-цель, если останется время)

## 8. Пошаговый план

### Этап 0 — Настройка проекта (3–5 ч)
- [x] Создать solution и структуру проектов из раздела 2
- [x] `docker-compose.yml` со скелетом: PostgreSQL + Kafka (KRaft-режим, без Zookeeper — проще поднять)
- [x] Пустой `Program.cs` в Api, подключение к БД, `dotnet ef` установлен, первая миграция (пустая) проходит

### Этап 1 — Домен и БД (6–10 ч)
- [ ] Сущности из раздела 3 в `Domain`
- [ ] `DbContext` + конфигурации сущностей (Fluent API) в `Infrastructure`
- [ ] Миграции, накатываются на Postgres из docker-compose
- [ ] Seed-скрипт с тестовыми данными (пара категорий, поставщиков, складов, товаров)

### Этап 2 — Базовый CRUD API (10–14 ч)
- [ ] Controllers + DTO для Category/Supplier/Warehouse/Product (без авторизации пока)
- [ ] Пагинация и фильтрация на `GET /api/products`
- [ ] Swagger поднят и работает, ручки проверены вручную

### Этап 3 — Складская логика (8–12 ч)
- [ ] `POST /api/stock/movements`: создание движения + пересчёт `StockItem.Quantity`
- [ ] Проверка порога `ReorderThreshold`, генерация внутреннего события (пока без Kafka — просто лог)
- [ ] `GET /api/stock`, `GET /api/stock/movements` с фильтрами

### Этап 4 — Kafka (10–14 ч)
- [ ] Producer в Api/Infrastructure: публикация события в `stock.low-stock-alerts` при срабатывании порога
- [ ] Отдельный проект `AlertWorker`: consumer, читает топик, пишет `LowStockAlert` в БД, логирует
- [ ] `GET /api/alerts`, `PATCH /api/alerts/{id}/resolve`
- [ ] Ручная проверка: движение → алерт в Kafka → запись в БД от воркера

### Этап 5 — Авторизация (6–8 ч)
- [ ] `POST /api/auth/register`, `POST /api/auth/login`, выдача JWT
- [ ] Роли Admin/Manager/Viewer, `[Authorize]` на пишущих эндпоинтах
- [ ] Swagger настроен на передачу Bearer-токена

### Этап 6 — Сквозные механизмы (6–8 ч)
- [ ] Middleware глобальной обработки исключений
- [ ] FluentValidation на DTO
- [ ] Serilog
- [ ] `/health`

### Этап 7 — Тесты (10–14 ч)
- [ ] Unit-тесты на пересчёт остатков и логику алерта
- [ ] Integration-тесты на 3–4 ключевых эндпоинта

### Этап 8 — Docker + демо-данные (4–6 ч)
- [ ] Dockerfile для Api и AlertWorker
- [ ] Полный `docker-compose up` поднимает всё и работает "из коробки"
- [ ] Скрипт/seed для демонстрационных данных

### Этап 9 — CI + README (4–6 ч)
- [ ] GitHub Actions: build + тесты на push
- [ ] README: архитектура, как поднять локально, скриншоты Swagger, GIF демонстрации потока "движение → алерт → воркер"

**Итого: ~70–100 ч** — сопоставимо с прошлой оценкой, реалистично при нескольких часах в будни + выходные, даже с full-time работой параллельно.

## 9. Порядок приоритета, если времени не хватит

Этапы 0–4 — это ядро, ради которого весь проект затевается (EF Core + Web API + Kafka). Если времени в обрез, 5–9 можно упростить: авторизацию оставить, но без тонких ролей; тесты — только unit; Docker — только API+Postgres+Kafka без полного CI. Урезать можно всё, кроме 0–4.
