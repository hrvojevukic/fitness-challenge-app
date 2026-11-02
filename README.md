# Fitness Challenge

A full-stack application for tracking sports activities: running, walking,
cycling, gym, swimming and daily steps. Each activity is converted into a unified
points score, users are ranked on a global leaderboard, and every user has a
personal dashboard with charts of their activity over time and their activity
share by sport.

- **Backend:** C# / ASP.NET Core Web API (.NET 8), Entity Framework Core, SQLite
- **Frontend:** Angular 18, Chart.js

## Project structure

```
backend/
  src/
    FitnessChallenge.Domain/          entities
    FitnessChallenge.Application/     business logic (services), DTOs, interfaces
    FitnessChallenge.Infrastructure/  EF Core, SQL queries, JWT token issuing
    FitnessChallenge.Api/             controllers, middleware, OpenAPI
  tests/
    FitnessChallenge.Tests/           unit and integration tests
frontend/                             Angular application
```

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm

## Running locally

Run the backend and the frontend in two separate terminals.

### Backend

```bash
cd backend/src/FitnessChallenge.Api
dotnet run
```

Runs on **http://localhost:5205**. On first run, it automatically creates a local
`fitness.db` SQLite file and fills it with sample data.

### Frontend

```bash
cd frontend
npm install
npm start
```

Runs on **http://localhost:4200**.

### Usage

Open http://localhost:4200, register a user, then log in with that first and last
name. Once logged in, you can log activities. The leaderboard and dashboard update
as data is added.

## API documentation

While the backend is running, Swagger UI is available at
**http://localhost:5205/swagger**. For protected endpoints, call
`POST /api/auth/login` and paste the returned token under **Authorize**.

Lists (`/api/leaderboard`, `/api/users`, `/api/activities`) are paginated with the
`page` and `pageSize` parameters (up to 100 per page).

## Tests

```bash
cd backend
dotnet test
```

```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```
