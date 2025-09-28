# ⚡ EV Charging Station Booking – Backend

A production-ready backend service for **EV Charging Station Booking**, built with:

- **.NET 8 Web API (C#)**  
- **MongoDB** (NoSQL)  
- **JWT Authentication** (with roles)  
- **Clean Architecture** (Domain, Application, Infrastructure, API)  
- **Unit Testing (xUnit + FluentAssertions)**  
- **FluentValidation** for request validation  
- **Serilog** logging  
- **Swagger UI** with JWT authentication support  

---

## 🌟 Features

✅ **Authentication & Authorization**
- Secure login with JWT Bearer tokens  
- Roles: **Backoffice**, **Operator**, **Owner**  
- Passwords hashed with BCrypt  
- Default seeded admin (`admin / Admin@123`)  

✅ **Owners**
- Register & manage EV owners  
- NIC used as unique identifier  
- CRUD operations via Backoffice  

✅ **Stations**
- Manage charging stations (AC/DC)  
- Define capacity (number of slots)  
- Prevent deactivation if active bookings exist  

✅ **Schedules**
- Manage station schedules per day  
- Define multiple time slots with capacity per slot  

✅ **Bookings**
- Full lifecycle management:
  - **Create** (within 7 days only)  
  - **Update / Cancel** (≥ 12h before session)  
  - **Approve** → generates a **QR token**  
  - **Scan QR** (Operator validates)  
  - **Finalize** session (mark as Completed)  
- Business rules enforced at service level  

✅ **Infrastructure**
- MongoDB repository pattern with indexes for performance & constraints  
- Centralized exception handling middleware  
- Structured logging with Serilog  
- Swagger with JWT integration  
- FluentValidation for DTO validation  
- Unit tests for booking rules & business logic  

---

## 🛠 Tech Stack

- **Backend**: ASP.NET Core 8 (Web API)  
- **Database**: MongoDB (6.x, Atlas or Docker/local)  
- **Auth**: JWT with role-based access  
- **Validation**: FluentValidation  
- **Testing**: xUnit + FluentAssertions  
- **Logging**: Serilog  
- **Deployment**: IIS / Docker-ready  

---

## 📂 Project Structure

```
ev-charger-booking/
│
├── EvCharging.Api/            # API Layer (Controllers, Middleware, Program.cs)
├── EvCharging.Application/    # DTOs, Interfaces, Validators
├── EvCharging.Domain/         # Entities, Enums, Base Classes
├── EvCharging.Infrastructure/ # MongoDB, Repositories, Services, Security
├── EvCharging.Tests/          # Unit tests (xUnit)
└── README.md                  # Documentation
```

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- [MongoDB](https://www.mongodb.com/try/download/community) (or Docker)  
- Git

Run MongoDB with Docker (optional):
```bash
docker run -d --name mongo-dev -p 27017:27017 -v mongo_data:/data/db mongo:6
```

---

### 2. Clone the Repository
```bash
git clone https://github.com/IT22601124/EVCS-WebService.git
cd ev-charger-booking
```

---

### 3. Configure App Settings

**EvCharging.Api/appsettings.Development.json**
```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "evcs_dev"
  },
  "Jwt": {
    "Issuer": "EvCharging",
    "Audience": "EvChargingClients",
    "Secret": "CHANGE_ME_SUPER_SECRET_256BIT_KEY",
    "ExpiryMinutes": 120
  },
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "AllowedHosts": "*"
}
```

⚠️ Replace `Jwt:Secret` with a strong key in production.

---

### 4. Restore & Build
```bash
dotnet restore
dotnet build
```

---

### 5. Run API
```bash
cd EvCharging.Api
dotnet run
```

- Swagger UI: [https://localhost:7147/swagger](https://localhost:7147/swagger)  
- API root: [https://localhost:7147/api](https://localhost:7147/api)  

---

## 🔑 Default Admin User

Seeded on first run:

- **Username:** `admin`  
- **Password:** `Admin@123`  
- **Role:** Backoffice  

---

## 📖 API Reference

### 🔐 Authentication
`POST /api/auth/login`  
Request:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```
Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
  "expiresAt": "2025-09-30T12:34:56Z",
  "role": "Backoffice",
  "username": "admin"
}
```

---

### 👤 Owners
- `POST /api/owners` (Backoffice) → Create owner
```json
{
  "nic": "923456789V",
  "fullName": "Kasun Perera",
  "email": "kasun@example.com",
  "phone": "0771234567"
}
```

- `GET /api/owners` → List owners  
- `GET /api/owners/{nic}` → Get owner by NIC  
- `PUT /api/owners/{nic}` (Backoffice) → Update owner  

---

### 🏭 Stations
- `POST /api/stations` (Backoffice)  
```json
{
  "name": "Fort Charger A",
  "address": "Colombo Fort",
  "latitude": 6.933,
  "longitude": 79.85,
  "type": "DC",
  "slots": 4
}
```

- `GET /api/stations` → List stations  
- `GET /api/stations/{id}` → Get station by ID  
- `PUT /api/stations/{id}` (Backoffice) → Update station  

---

### 📅 Schedules
- `PUT /api/schedules` (Backoffice/Operator)  
```json
{
  "stationId": "STATION123",
  "date": "2025-10-01",
  "slots": [
    { "start": "08:00:00", "end": "09:00:00", "available": true, "capacity": 2 },
    { "start": "09:00:00", "end": "10:00:00", "available": true, "capacity": 2 }
  ]
}
```

- `GET /api/schedules?stationId=STATION123&date=2025-10-01`

---

### 📖 Bookings
- `POST /api/bookings` (Owner/Backoffice)  
```json
{
  "nic": "923456789V",
  "stationId": "STATION123",
  "date": "2025-10-01",
  "start": "08:00:00",
  "end": "09:00:00"
}
```

- `GET /api/bookings/{id}` → Get booking  
- `GET /api/bookings/by-owner/{nic}` → List bookings by owner  
- `PUT /api/bookings/{id}` → Update booking  
- `DELETE /api/bookings/{id}` → Cancel booking  
- `POST /api/bookings/{id}/approve` (Backoffice) → Approve booking & generate QR token  
- `POST /api/bookings/scan` (Operator/Backoffice)  
```json
{ "qrToken": "abc123-qr-guid" }
```
- `POST /api/bookings/{id}/finalize` (Operator/Backoffice) → Finalize booking  

---

## 🔎 Swagger with JWT Support

Swagger is enabled with JWT Bearer authentication.  

1. Login via `/api/auth/login`  
2. Copy the token (without `"Bearer "` prefix)  
3. In Swagger UI → Click **Authorize** → Enter `Bearer <your-token>`  
4. You can now call all protected endpoints directly from Swagger.

---

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Sample tested rules:
- Booking date ≤ 7 days  
- Cannot cancel within 12 hours  
- Capacity enforcement per slot  
- Station deactivation guard  

---

## 📦 Deployment (IIS)

1. Publish:
```bash
dotnet publish EvCharging.Api -c Release -o publish
```

2. IIS setup:
- Add Website → Path = `publish` folder  
- Binding `https` (with cert)  
- App Pool: **No Managed Code** / In-Process  
- Set env: `ASPNETCORE_ENVIRONMENT=Production`  

3. Update `appsettings.json` with production Mongo + JWT.

---

## 🎯 Summary

This backend provides:
- Secure authentication & roles  
- Owner, Station, Schedule, Booking lifecycle APIs  
- Enforced business rules (7 days, 12 hours, capacity, QR flow)  
- MongoDB persistence with indexes  
- Validation, logging, testing, Swagger docs  
- IIS-ready deployment  

---
