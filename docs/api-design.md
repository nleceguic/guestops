# API Design — Smart Guest Operations Platform

## Base URL

```
https://localhost:7187/api      (development)
https://api.guestops.com/api   (production)
```

## Authentication

All protected endpoints require a JWT Bearer token in the `Authorization` header:

```
Authorization: Bearer <token>
```

Obtain a token via `POST /api/auth/login`. Tokens expire after 60 minutes (configurable in `appsettings.json`).

---

## Conventions

| Convention | Value |
|---|---|
| Format | JSON |
| Date format | `yyyy-MM-dd` (DateOnly fields) |
| DateTime format | ISO 8601 — `2026-03-15T10:00:00Z` |
| Currency | Decimal with 2 decimal places |
| Soft deletes | Status change instead of physical DELETE |
| Error format | `{ "status": 400, "error": "message", "path": "/api/..." }` |

### HTTP Status Codes

| Code | When |
|---|---|
| 200 OK | Successful GET, PUT, PATCH |
| 201 Created | Successful POST |
| 204 No Content | Successful DELETE |
| 400 Bad Request | Validation error, invalid field value |
| 401 Unauthorized | Missing or expired token |
| 403 Forbidden | Valid token but insufficient role |
| 404 Not Found | Resource does not exist |
| 409 Conflict | Business rule violation (duplicate, active reservations, etc.) |
| 500 Internal Server Error | Unhandled exception |

---

## Modules

- [Auth](#auth)
- [Reservations](#reservations)
- [Apartments](#apartments)
- [Cleaning Schedules](#cleaning-schedules)
- [Incidents](#incidents)
- [Payments](#payments)
- [Guest Messages](#guest-messages)

---

## Auth

### POST /api/auth/register
Creates a new user account.

**Auth:** Public

**Request:**
```json
{
  "firstName": "Joan",
  "lastName": "García",
  "email": "joan@test.com",
  "password": "Admin1234!",
  "role": "Guest"
}
```

**Roles:** `Guest` | `Owner` | `Operator` | `Admin`

**Response 201:**
```json
{
  "accessToken": "eyJ...",
  "expiresAt": "2026-03-15T11:00:00Z",
  "user": {
    "id": 1,
    "email": "joan@test.com",
    "firstName": "Joan",
    "lastName": "García",
    "role": "Guest",
    "language": "es",
    "isActive": true,
    "createdAt": "2026-03-15T10:00:00Z"
  }
}
```

**Errors:** `409` email already registered · `400` invalid role

---

### POST /api/auth/login
Authenticates a user and returns a JWT token.

**Auth:** Public

**Request:**
```json
{
  "email": "admin@abartment.com",
  "password": "Admin1234!"
}
```

**Response 200:** Same as register response.

**Errors:** `401` invalid credentials

---

### GET /api/auth/me
Returns the currently authenticated user.

**Auth:** JWT

**Response 200:**
```json
{
  "id": 1,
  "email": "admin@abartment.com",
  "firstName": "Carlos",
  "lastName": "Mendoza",
  "role": "Admin",
  "language": "es",
  "isActive": true,
  "createdAt": "2026-03-15T10:00:00Z"
}
```

---

### PUT /api/auth/change-password
Changes the password of the authenticated user.

**Auth:** JWT

**Request:**
```json
{
  "currentPassword": "Admin1234!",
  "newPassword": "NewPass5678!"
}
```

**Response 200:** Updated `UserDto`

**Errors:** `401` incorrect current password

---

## Reservations

### GET /api/reservations
Returns all reservations. Operators and Admins see all; Guests see only their own.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Response 200:**
```json
[
  {
    "id": 1,
    "apartmentId": 1,
    "apartmentName": "Ático Gràcia",
    "guestFullName": "John Smith",
    "checkInDate": "2026-04-01",
    "checkOutDate": "2026-04-05",
    "nights": 4,
    "totalAmount": 480.00,
    "status": "Confirmed",
    "channel": "Airbnb"
  }
]
```

---

### GET /api/reservations/{id}
Returns full detail of a reservation.

**Auth:** JWT

**Response 200:**
```json
{
  "id": 1,
  "apartmentId": 1,
  "apartmentName": "Ático Gràcia",
  "apartmentAddress": "Carrer de Verdi 28, 4º 1ª",
  "guestId": 6,
  "guestFullName": "John Smith",
  "guestEmail": "guest1@test.com",
  "channel": "Airbnb",
  "checkInDate": "2026-04-01",
  "checkOutDate": "2026-04-05",
  "nights": 4,
  "numGuests": 2,
  "totalAmount": 480.00,
  "currency": "EUR",
  "status": "Confirmed",
  "checkInMethod": "SmartLock",
  "specialRequests": null,
  "createdAt": "2026-03-01T10:00:00Z",
  "cancelledAt": null
}
```

**Errors:** `404` not found

---

### GET /api/reservations/my
Returns reservations of the authenticated guest.

**Auth:** JWT

---

### GET /api/reservations/apartment/{apartmentId}
Returns reservations of a specific apartment.

**Auth:** JWT · Roles: `Admin`, `Operator`, `Owner`

---

### POST /api/reservations
Creates a new reservation. Price is calculated automatically (`BaseNightlyRate × nights`).

**Auth:** JWT

**Request:**
```json
{
  "apartmentId": 1,
  "guestId": 6,
  "checkInDate": "2026-04-01",
  "checkOutDate": "2026-04-05",
  "numGuests": 2,
  "channel": "Direct",
  "checkInMethod": "SmartLock",
  "specialRequests": "Late check-in"
}
```

**CheckInMethod values:** `SmartLock` | `KeyBox` | `OfficePickup`

**Response 201:** Full `ReservationDto`

**Errors:** `409` overlapping dates · `409` exceeds max guests · `404` apartment not found

---

### PUT /api/reservations/{id}
Updates mutable fields of a reservation.

**Auth:** JWT

**Request:**
```json
{
  "numGuests": 3,
  "checkInMethod": "KeyBox",
  "specialRequests": "Cot for baby"
}
```

All fields optional.

---

### PATCH /api/reservations/{id}/status
Changes reservation status.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{ "status": "CheckedIn" }
```

**Allowed transitions:**

| From | To |
|---|---|
| `Confirmed` | `CheckedIn` |
| `Confirmed` | `Cancelled` |
| `CheckedIn` | `CheckedOut` |

**Errors:** `400` invalid transition

---

### DELETE /api/reservations/{id}
Cancels a reservation (soft delete → `Cancelled`).

**Auth:** JWT

**Response 204**

---

## Apartments

### GET /api/apartments
Returns all apartments.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### GET /api/apartments/{id}
Returns full apartment detail including owner info.

**Auth:** JWT

**Response 200:**
```json
{
  "id": 1,
  "ownerId": 4,
  "ownerFullName": "Isabel Romero",
  "ownerEmail": "owner1@abartment.com",
  "internalCode": "BCN-001",
  "name": "Ático Gràcia",
  "addressLine": "Carrer de Verdi 28, 4º 1ª",
  "district": "Gràcia",
  "bedrooms": 2,
  "maxGuests": 4,
  "baseNightlyRate": 120.00,
  "floorArea": 75.0,
  "latitude": 41.4036,
  "longitude": 2.1577,
  "smartLockCode": "SL-1001",
  "status": "Active",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

---

### GET /api/apartments/my
Returns apartments owned by the authenticated user.

**Auth:** JWT · Roles: `Owner`, `Admin`

---

### GET /api/apartments/by-status/{status}
Filters apartments by status.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Status values:** `Active` | `Inactive` | `UnderMaintenance`

---

### GET /api/apartments/{id}/metrics
Returns performance metrics for the Owner Dashboard.

**Auth:** JWT · Roles: `Owner`, `Admin`, `Operator`

**Response 200:**
```json
{
  "apartmentId": 1,
  "name": "Ático Gràcia",
  "internalCode": "BCN-001",
  "totalReservations": 12,
  "activeReservations": 1,
  "totalRevenue": 5760.00,
  "averageNightlyRate": 118.50,
  "occupancyRatePercent": 68.5,
  "pendingIncidents": 1
}
```

`occupancyRatePercent` is calculated over the last 90 days.

---

### POST /api/apartments
Creates a new apartment.

**Auth:** JWT · Roles: `Admin`

**Request:**
```json
{
  "ownerId": 4,
  "internalCode": "BCN-011",
  "name": "Piso Gràcia",
  "addressLine": "Carrer de Verdi 50, 1º",
  "district": "Gràcia",
  "bedrooms": 2,
  "maxGuests": 4,
  "baseNightlyRate": 115.00,
  "floorArea": 70.0,
  "smartLockCode": "SL-1011"
}
```

**Errors:** `409` duplicate `internalCode` · `409` owner not found or invalid role

---

### PUT /api/apartments/{id}
Updates apartment data. All fields optional.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### PATCH /api/apartments/{id}/status
Changes apartment status.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{ "status": "UnderMaintenance" }
```

**Errors:** `409` cannot deactivate with active reservations

---

### DELETE /api/apartments/{id}
Soft delete — marks apartment as `Inactive`.

**Auth:** JWT · Roles: `Admin`

**Errors:** `409` has active reservations

---

## Cleaning Schedules

### GET /api/cleaningschedules/daily/{date}
Returns the daily planning for the Operations Dashboard.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Date format:** `yyyy-MM-dd` (e.g. `2026-04-01`)

**Response 200:**
```json
{
  "date": "2026-04-01",
  "total": 5,
  "scheduled": 3,
  "inProgress": 1,
  "done": 1,
  "skipped": 0,
  "cleanings": [
    {
      "id": 1,
      "apartmentName": "Ático Gràcia",
      "apartmentDistrict": "Gràcia",
      "assignedToFullName": "María García",
      "scheduledDate": "2026-04-01",
      "scheduledTime": "11:00:00",
      "type": "Checkout",
      "status": "Scheduled"
    }
  ]
}
```

---

### GET /api/cleaningschedules/apartment/{apartmentId}
Returns cleaning history for an apartment.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### GET /api/cleaningschedules/operator/{operatorId}
Returns cleanings assigned to a specific operator.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### GET /api/cleaningschedules/by-status/{status}
Filters cleanings by status.

**Status values:** `Scheduled` | `InProgress` | `Done` | `Skipped`

---

### POST /api/cleaningschedules
Creates a cleaning manually.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{
  "apartmentId": 1,
  "scheduledDate": "2026-04-10",
  "scheduledTime": "11:00:00",
  "type": "Deep",
  "reservationId": 1,
  "assignedToId": 2,
  "notes": "Use fragrance-free products"
}
```

**Type values:** `Checkout` | `Midstay` | `Maintenance` | `Deep`

**Errors:** `400` date in the past · `409` assigned user is not Operator or Admin

---

### POST /api/cleaningschedules/generate-checkout/{reservationId}
Auto-generates a checkout cleaning scheduled for the reservation's check-out date at 11:00.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Response 201:** Full `CleaningScheduleDto`

---

### PATCH /api/cleaningschedules/{id}/status
Changes cleaning status.

**Request:**
```json
{ "status": "InProgress", "notes": "Started" }
```

**Allowed transitions:**

| From | To |
|---|---|
| `Scheduled` | `InProgress` |
| `Scheduled` | `Skipped` |
| `InProgress` | `Done` |
| `InProgress` | `Skipped` |

---

### DELETE /api/cleaningschedules/{id}
Soft delete — marks as `Skipped`.

**Errors:** `409` cannot delete an InProgress or Done cleaning

---

## Incidents

### GET /api/incidents/dashboard
Returns the Operations Dashboard summary.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Response 200:**
```json
{
  "totalOpen": 4,
  "totalInProgress": 2,
  "totalResolved": 8,
  "criticalOpen": 1,
  "highOpen": 2,
  "unassignedOpen": 0,
  "criticalAndHigh": [...],
  "recentlyOpened": [...]
}
```

---

### GET /api/incidents/by-status/{status}
Filters incidents by status.

**Status values:** `Open` | `InProgress` | `Resolved` | `Closed`

---

### GET /api/incidents/by-category/{category}
Filters incidents by category.

**Category values:** `Maintenance` | `Cleaning` | `Complaint` | `Other`

---

### GET /api/incidents/by-priority/{priority}
Filters incidents by priority.

**Priority values:** `Low` | `Medium` | `High` | `Critical`

---

### POST /api/incidents
Creates a new incident. Auto-assigns to the first available operator.

**Auth:** JWT (any authenticated user can report)

**Request:**
```json
{
  "apartmentId": 1,
  "category": "Maintenance",
  "priority": "High",
  "title": "Heating not working",
  "description": "Guest reports heating is off. Indoor temp 14°C.",
  "reservationId": 1
}
```

**Response 201:** Full `IncidentDto` with `assignedToFullName` populated

---

### PATCH /api/incidents/{id}/assign
Manually assigns an operator. Automatically transitions `Open → InProgress`.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{ "assignedToId": 2 }
```

---

### PATCH /api/incidents/{id}/status
Changes incident status with optional resolution note.

**Request:**
```json
{
  "status": "Resolved",
  "resolutionNote": "Replaced thermostat. Heating working."
}
```

**Allowed transitions:**

| From | To |
|---|---|
| `Open` | `InProgress` |
| `Open` | `Closed` |
| `InProgress` | `Resolved` |
| `InProgress` | `Closed` |
| `Resolved` | `Closed` |

When resolved, `resolvedAt` is set automatically and the note is appended to `description`.

---

### PATCH /api/incidents/{id}/zendesk
Associates a Zendesk ticket ID with the incident.

**Request:**
```json
{ "zendeskTicketId": "ZD-00042" }
```

---

## Payments

### GET /api/payments/reservation/{reservationId}
Returns the full financial summary of a reservation.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Response 200:**
```json
{
  "reservationId": 1,
  "apartmentName": "Ático Gràcia",
  "guestFullName": "John Smith",
  "totalAmount": 480.00,
  "totalPaid": 480.00,
  "pendingAmount": 0.00,
  "isFullyPaid": true,
  "payments": [...]
}
```

---

### GET /api/payments/pending
Returns all payments awaiting confirmation.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### POST /api/payments
Registers a new payment. Cash payments are automatically confirmed.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{
  "reservationId": 1,
  "amount": 150.00,
  "type": "Deposit",
  "method": "BankTransfer",
  "transactionRef": "TRANS-ES-20260401"
}
```

**Type values:** `Deposit` | `Balance` | `Refund` | `Extra`

**Method values:** `Card` | `BankTransfer` | `Cash` | `Stripe`

**Errors:** `409` amount exceeds reservation total · `409` cancelled reservation

---

### POST /api/payments/{id}/confirm
Confirms a pending payment (e.g. bank transfer received).

**Request:**
```json
{ "transactionRef": "TRANS-ES-20260401-CONF" }
```

**Errors:** `400` payment is not in Pending status

---

### POST /api/payments/{id}/fail
Marks a pending payment as failed.

**Request:**
```json
{ "reason": "Card declined by bank" }
```

---

### POST /api/payments/{id}/refund
Processes a partial or full refund. Creates a new `Refund` payment with negative amount.

**Auth:** JWT · Roles: `Admin`

**Request:**
```json
{
  "amount": 50.00,
  "reason": "Discount for incident during stay"
}
```

**Errors:** `409` refund exceeds original payment · `409` payment not Completed

---

## Guest Messages

### POST /api/guestmessages/inbound
Processes an inbound guest message. Intended to be called by WhatsApp/Email webhooks.

**Auth:** Public (AllowAnonymous)

**Request:**
```json
{
  "guestId": 6,
  "reservationId": 1,
  "channel": "WhatsApp",
  "body": "What time can I check in?"
}
```

**Channel values:** `WhatsApp` | `Email` | `Chat` | `Phone`

**Response 200:**
```json
{
  "inboundMessage": { "id": 1, "body": "What time can I check in?", ... },
  "autoReply": {
    "id": 2,
    "body": "Hi John, check-in is from 3:00 PM...",
    "isAutoReply": true,
    "aiConfidence": 88.0,
    "detectedTopic": "checkin"
  },
  "wasAutoReplied": true,
  "incidentCreated": false,
  "incidentId": null,
  "detectedTopic": "checkin",
  "aiConfidence": 88.0
}
```

**AI topics:** `checkin` | `checkout` | `wifi` | `transport` | `food` | `incident` | `rules` | `unknown`

When `detectedTopic` is `incident`, an `Incident` is automatically created and `incidentCreated: true`.

---

### GET /api/guestmessages/recent
Returns the last N inbound messages — inbox for the Operations Dashboard.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Query params:** `?count=20` (default 20)

---

### GET /api/guestmessages/pending-reply
Returns inbound messages with no reply yet — require human attention.

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### GET /api/guestmessages/reservation/{reservationId}
Returns the full conversation thread for a reservation (inbound + outbound, ordered by time).

**Auth:** JWT · Roles: `Admin`, `Operator`

---

### POST /api/guestmessages/reservation/{reservationId}/reply
Sends a manual reply from an operator.

**Auth:** JWT · Roles: `Admin`, `Operator`

**Request:**
```json
{
  "operatorId": 2,
  "body": "Hi Joan, of course — what would you like to know?"
}
```

**Response 201:** `GuestMessageDto` with `isAutoReply: false`, `direction: "Outbound"`