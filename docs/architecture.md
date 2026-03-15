# Architecture — Smart Guest Operations Platform

## Overview

Full-stack platform for managing 500+ tourist apartments. Built with .NET 8 Clean Architecture backend and Angular 17 frontend, featuring an AI assistant that automatically responds to guest messages and creates maintenance incidents.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| Architecture | Clean Architecture (4 layers) |
| Database | SQL Server + Entity Framework Core 8 |
| ORM | EF Core Fluent API |
| Auth | JWT Bearer + BCrypt |
| Frontend | Angular 17 (Standalone Components) |
| UI Library | Angular Material |
| AI Assistant | Anthropic Claude API (mock in dev) |

---

## Project Structure

```
smart-guest-operations-platform/
├── backend/
│   └── ABAPpartment/
│       ├── ABAPpartment.Domain/          # Entities, Enums, Interfaces
│       ├── ABAPpartment.Application/     # Services, DTOs, Interfaces
│       ├── ABAPpartment.Infrastructure/  # EF Core, Repositories, AI
│       └── ABAPpartment.API/             # Controllers, Middleware, Program.cs
└── frontend/
    └── ab-apartment-frontend/
        └── src/app/
            ├── core/                     # Models, Services, Guards, Interceptors
            ├── features/                 # Feature modules (lazy loaded)
            │   ├── auth/
            │   ├── reservations/
            │   ├── operations/
            │   ├── messaging/
            │   └── owner/
            └── shell/                    # Layout, Sidebar navigation
```

---

## Backend Layers

### Domain
No external dependencies. Contains:
- **Entities** — `User`, `Apartment`, `Reservation`, `Payment`, `Incident`, `CleaningSchedule`, `GuestMessage`
- **Interfaces** — Repository contracts (`IUserRepository`, `IReservationRepository`, etc.)
- **Constants** — Status and type enumerations per module

### Application
References Domain only. Contains:
- **Services** — Business logic (`AuthService`, `ReservationService`, `ApartmentService`, etc.)
- **DTOs** — Request/Response records per module
- **Interfaces** — Service contracts + `IAIAssistantService`

### Infrastructure
References Application. Contains:
- **Repositories** — EF Core implementations
- **Persistence** — `AppDbContext`, Fluent API configurations per entity
- **AI** — `MockAIAssistantService` (dev) / `ClaudeAIAssistantService` (prod)

### API
References Application + Infrastructure. Contains:
- **Controllers** — REST endpoints with JWT authorization
- **Middleware** — `GlobalExceptionMiddleware` (maps exceptions to HTTP codes)
- **Extensions** — `ServiceExtensions` (DI registration, JWT config, Swagger)

---

## Database Schema

9 tables in `ABAPpartmentDB`:

```
Users
├── Apartments (OwnerId → Users)
│   ├── Reservations (ApartmentId → Apartments, GuestId → Users)
│   │   ├── Payments (ReservationId → Reservations)
│   │   ├── Incidents (ApartmentId → Apartments, ReservationId → Reservations)
│   │   ├── CleaningSchedules (ApartmentId → Apartments, ReservationId → Reservations)
│   │   └── GuestMessages (ReservationId → Reservations, GuestId → Users)
│   └── OccupancyForecasts (ApartmentId → Apartments) ← Fase 4
└── AuditLogs
```

---

## API Modules

| Module | Endpoints | Auth |
|---|---|---|
| Auth | `/api/auth` — register, login, me, change-password | Public / JWT |
| Reservations | `/api/reservations` — CRUD, status transitions | JWT + Roles |
| Apartments | `/api/apartments` — CRUD, metrics dashboard | JWT + Roles |
| CleaningSchedules | `/api/cleaningschedules` — planning, daily view | Admin, Operator |
| Incidents | `/api/incidents` — CRUD, assign, Zendesk sync | JWT + Roles |
| Payments | `/api/payments` — confirm, refund, reservation summary | Admin, Operator |
| GuestMessages | `/api/guestmessages` — inbound processing, AI reply | Public / JWT |

---

## User Roles

| Role | Permissions |
|---|---|
| `Admin` | Full access to all endpoints |
| `Operator` | Operations — cleaning, incidents, messages |
| `Owner` | Own apartments metrics and reservations |
| `Guest` | Own reservations, send messages |

---

## AI Assistant

The `GuestMessageService` orchestrates the AI flow on every inbound message:

```
Inbound message
      │
      ▼
Save to GuestMessages (Inbound)
      │
      ▼
IAIAssistantService.GetResponseAsync()
      │
      ├─ Confidence ≥ 70% ──► Save auto-reply (Outbound, IsAutoReply=true)
      │                              │
      │                              └─ ShouldCreateIncident=true ──► Create Incident automatically
      │
      └─ Confidence < 70% ──► No auto-reply → appears in pending-reply queue
```

**Topic detection:** check-in, check-out, wifi, transport, food, incident, rules, unknown

**Switching to Claude API (production):**
1. Add `"AnthropicApiKey": "sk-ant-..."` in `appsettings.json`
2. Replace `MockAIAssistantService` with `ClaudeAIAssistantService` in `ServiceExtensions.cs`

---

## Frontend Architecture

Angular 17 with standalone components and lazy loading per feature:

```
AppComponent (router-outlet)
├── /login → LoginComponent
└── ShellComponent (sidebar + router-outlet)
    ├── /reservations → ReservationListComponent
    │                   ReservationDetailComponent
    ├── /operations → OperationsDashboardComponent
    │                 ├── CleaningPanelComponent
    │                 ├── IncidentsPanelComponent
    │                 └── MessagesPanelComponent
    ├── /messaging → (Phase 3)
    └── /owner → (Phase 4)
```

**Core services:**
- `AuthService` — JWT token + Angular Signals for reactive auth state
- `ReservationService` — REST calls to `/api/reservations`
- `OperationsService` — REST calls to cleaning, incidents, messages endpoints

**Auth interceptor** automatically attaches `Authorization: Bearer <token>` to every HTTP request.

---

## Development Setup

### Backend
```bash
# SQL Server connection (appsettings.json)
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ABAPpartmentDB;Trusted_Connection=True;TrustServerCertificate=True"
}

# Run
cd backend/ABAPpartment
dotnet run --project ABAPpartment.API
# Swagger: https://localhost:7187/swagger
```

### Frontend
```bash
cd frontend/ab-apartment-frontend
npm install
ng serve
# App: http://localhost:4200
# Proxy: /api → https://localhost:7187
```

### Seed Data
```bash
# Execute in SSMS after CreateDatabase.sql
seed-data.sql
```

**Dev credentials** (password: `Admin1234!` for all):

| Email | Role |
|---|---|
| `admin@abartment.com` | Admin |
| `operario1@abartment.com` | Operator |
| `owner1@abartment.com` | Owner |
| `guest1@test.com` | Guest |

---

## Roadmap

| Phase | Status | Description |
|---|---|---|
| Phase 1 | ✅ Complete | Backend API — all 7 modules |
| Phase 2 | 🔄 In progress | Angular frontend — Reservations dashboard |
| Phase 3 | Pending | Operations Dashboard |
| Phase 4 | Pending | Guest Messaging panel, Owner Dashboard |
| Phase 5 | Pending | Connect Claude AI, predictive analytics |