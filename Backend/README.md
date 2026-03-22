# SmartTimeManager API

- POST `/api/auth/register`
- POST `/api/auth/login`
- GET `/api/profile`
- PUT `/api/profile`
- POST `/api/timer/start`
- POST `/api/timer/stop`
- POST `/api/activity`
- POST `/api/violations`
- GET `/api/reports/day`
- GET `/api/admin/employees`
- GET `/api/admin/employees/{id}`

## Что нужно перед запуском

1. Установить .NET 8 SDK.
2. Установить PostgreSQL.
3. Создать БД и таблицы по вашей схеме.
4. В `appsettings.json` прописать свою строку подключения.
5. Задать свой `Jwt:SecretKey`.

## Запуск

```bash
 dotnet restore
 dotnet run
```

Swagger откроется по адресу:

```text
http://localhost:5000/swagger
https://localhost:5001/swagger
```

## Важно

Этот архив сделан под вашу текущую схему БД:

- `users`
- `tech_stack_items`
- `work_sessions`
- `activity_records`
- `violations`
- `daily_reports`
- `refresh_tokens`

Если вы создали БД по другой схеме, нужно будет подправить `AppDbContext` и модели.

## Что ещё может потребоваться

- миграции EF Core, если захотите перейти на code-first;
- refresh endpoint, если захотите полноценное обновление access token;
- более умная классификация `work/rest`;
- автопауза таймера и dispute flow.
