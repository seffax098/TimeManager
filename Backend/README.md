# Backend API

## Маршруты API

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Profile
- `GET /api/profile`
- `PUT /api/profile`

### Timer
- `POST /api/timer/start`
- `POST /api/timer/stop`

### Activity
- `POST /api/activity`
- `PUT /api/activity/activityId/verdict`

### Violations
- `POST /api/violations`

### Reports
- `GET /api/reports/day`

### Admin
- `GET /api/admin/employees`
- `GET /api/admin/employees/{id}`

## Что нужно перед запуском

1. Установить .NET 8 SDK
2. Установить PostgreSQL
3. Создать базу данных
4. Прописать строку подключения в `appsettings.json`
5. Указать свой `Jwt:SecretKey`

## База данных

```bash
dotnet restore
dotnet ef database update
```

## Запуск проекта

```bash
dotnet restore
dotnet run
```

## Swagger

```text
http://localhost:5000/swagger
```