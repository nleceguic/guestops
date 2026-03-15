# Roadmap — Smart Guest Operations Platform

## Vision

Build a fully automated operations platform for AB Apartment Barcelona that reduces manual workload by 80%, responds to guest messages automatically via AI, and gives owners and operators real-time visibility into their properties.

---

## Current Status

| Layer | Status |
|---|---|
| Backend API | ✅ Complete — 7 modules, all endpoints tested |
| Database | ✅ Complete — 9 tables, seed data |
| Frontend — Reservations | ✅ Complete — list + detail + status actions |
| Frontend — Operations Dashboard | 🔄 In progress |
| Frontend — Guest Messaging | ⏳ Pending |
| Frontend — Owner Dashboard | ⏳ Pending |
| Claude AI integration | ⏳ Pending |
| Deployment | ⏳ Pending |

---

## Phase 1 — Backend API ✅

**Goal:** Solid REST API with Clean Architecture, JWT auth, and all core business modules.

### Completed
- [x] Clean Architecture setup — Domain, Application, Infrastructure, API
- [x] SQL Server database — 9 tables, Fluent API configurations
- [x] JWT authentication with BCrypt password hashing
- [x] Role-based authorization — Admin, Operator, Owner, Guest
- [x] Global exception middleware — consistent error responses
- [x] Swagger with Bearer auth

### Modules
- [x] **Auth** — register, login, me, change-password
- [x] **Reservations** — CRUD, overlap validation, status transitions, price auto-calculation
- [x] **Apartments** — CRUD, Owner Dashboard metrics, occupancy rate (last 90 days)
- [x] **CleaningSchedules** — daily planning, auto-generate checkout cleanings
- [x] **Incidents** — Smart Incident Manager, auto-assignment, Zendesk sync
- [x] **Payments** — deposits, balance, refunds, financial summary per reservation
- [x] **GuestMessages + AI Assistant** — inbound processing, mock AI, auto-incident creation

---

## Phase 2 — Angular Frontend 🔄

**Goal:** Operational dashboards for Admin and Operator roles covering day-to-day management.

### Completed
- [x] Project setup — Angular 17, Angular Material, proxy config
- [x] Shell — sidebar navigation, JWT interceptor, auth guard
- [x] Auth — login page with role-aware routing
- [x] **Reservations** — list with filters, detail with status actions

### In Progress
- [ ] **Operations Dashboard**
  - [ ] Cleaning panel — daily planning with date picker, status updates
  - [ ] Incidents panel — KPI cards, critical/high table, recently opened
  - [ ] Messages panel — pending reply inbox, recent messages with AI metadata

### Pending
- [ ] **Guest Messaging panel**
  - [ ] Conversation thread view per reservation
  - [ ] Manual reply form
  - [ ] AI confidence and topic indicators
- [ ] **Owner Dashboard**
  - [ ] Apartment metrics cards (occupancy, revenue, active reservations)
  - [ ] Reservations list filtered by owner
  - [ ] Pending incidents per apartment

---

## Phase 3 — Claude AI Integration ⏳

**Goal:** Replace the mock AI with real Claude API calls for intelligent, context-aware guest responses.

### Tasks
- [ ] Create `ClaudeAIAssistantService` implementing `IAIAssistantService`
- [ ] Build system prompt with apartment context, check-in/out info, house rules
- [ ] Implement topic detection using Claude's reasoning
- [ ] Add confidence scoring based on response certainty
- [ ] Handle multi-language responses (ES, EN, FR, DE, CA)
- [ ] Add conversation history support for multi-turn exchanges
- [ ] Implement fallback to human agent when confidence < 70%
- [ ] Add `AnthropicApiKey` to `appsettings.json` and environment config

### Activation (one line change)
```csharp
// ServiceExtensions.cs
// Before:
services.AddScoped<IAIAssistantService, MockAIAssistantService>();
// After:
services.AddScoped<IAIAssistantService, ClaudeAIAssistantService>();
```

---

## Phase 4 — Predictive Analytics ⏳

**Goal:** ML-powered occupancy forecasting and revenue optimization using the `OccupancyForecasts` table.

### Tasks
- [ ] Implement `OccupancyForecast` entity and repository (stub already in DB)
- [ ] Build occupancy prediction model based on historical reservation data
- [ ] Dynamic pricing recommendations based on demand and seasonality
- [ ] Revenue optimization dashboard for owners
- [ ] Seasonal trend analysis — peak periods, low occupancy alerts
- [ ] Competitor pricing integration (optional)

---

## Phase 5 — Deployment ⏳

**Goal:** Production-ready deployment with CI/CD, monitoring, and security hardening.

### Backend
- [ ] Dockerize .NET API — `Dockerfile` + `docker-compose.yml`
- [ ] Environment-specific `appsettings` — dev, staging, production
- [ ] EF Core migrations (replace manual SQL scripts)
- [ ] Azure App Service or AWS ECS deployment
- [ ] SQL Server on Azure or AWS RDS
- [ ] HTTPS certificate + custom domain

### Frontend
- [ ] Production build — `ng build --configuration production`
- [ ] Azure Static Web Apps or AWS Amplify
- [ ] CDN for static assets
- [ ] Environment variables for API URL

### DevOps
- [ ] GitHub Actions CI/CD pipeline
  - [ ] Backend — build, test, deploy on push to `main`
  - [ ] Frontend — lint, build, deploy on push to `main`
- [ ] Health check endpoint — `GET /api/health`
- [ ] Application Insights or CloudWatch monitoring
- [ ] Sentry error tracking

### Security
- [ ] Rotate JWT secret to 256-bit key
- [ ] Refresh token implementation
- [ ] Rate limiting on auth endpoints
- [ ] CORS policy for production domain
- [ ] SQL injection audit (EF Core parameterized queries — already safe)
- [ ] API key management for Anthropic and third-party services

---

## Phase 6 — Integrations ⏳

**Goal:** Connect the platform with external tools already used by AB Apartment Barcelona.

### Zendesk
- [ ] Webhook to auto-create Zendesk tickets from Critical/High incidents
- [ ] Bidirectional sync — status updates from Zendesk reflected in platform
- [ ] Agent notes sync

### Channel Manager
- [ ] Airbnb API — sync reservations automatically
- [ ] Booking.com API — sync reservations automatically
- [ ] iCal feed for other channels

### Communication
- [ ] WhatsApp Business API webhook — route inbound messages to `POST /api/guestmessages/inbound`
- [ ] SendGrid / Mailgun — email notifications for check-in instructions, incident updates
- [ ] SMS fallback for critical incidents

### Smart Lock
- [ ] Igloohome or Salto API — auto-generate access codes per reservation
- [ ] Code expiry aligned with check-out date
- [ ] Emergency override from Operations Dashboard

---

## Backlog

Ideas and improvements not yet scheduled:

- [ ] Mobile app — React Native or Angular PWA for operators in the field
- [ ] Guest-facing web portal — check-in, messages, apartment info
- [ ] Multi-language admin interface (currently Spanish only)
- [ ] Bulk operations — assign multiple cleanings at once
- [ ] Cleaning performance reports — time per cleaning, operator stats
- [ ] Automated invoice generation for owners (monthly PDF)
- [ ] Dark mode
- [ ] Offline support for operators (PWA service worker)

---

## Changelog

### v0.2.0 — 2026-03-15
- Angular frontend — Reservations module (list, detail, status actions)
- Seed data — 10 users, 10 apartments, 15 reservations, full mock data

### v0.1.0 — 2026-03-12
- .NET 8 Clean Architecture backend
- All 7 API modules implemented and tested
- JWT auth, role-based authorization
- SQL Server database with 9 tables
- Mock AI Assistant with topic detection and auto-incident creation