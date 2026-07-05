# PayCentral Corporate Expense Card Platform

A production-ready proof of concept for the PayCentral Corporate Expense Card platform built for the Senior Full Stack Developer technical assessment.

---

## Technology Stack

**Backend**
- .NET 8, ASP.NET Core Web API
- Clean Architecture with CQRS and MediatR
- Entity Framework Core 8 with SQL Server
- JWT Authentication with Refresh Tokens
- FluentValidation, Serilog, SignalR
- xUnit, Moq, FluentAssertions

**Frontend**
- React 19 with TypeScript
- Vite, TanStack Query, React Router
- Tailwind CSS, Lucide React
- SignalR Client for real-time alerts

---

## Architecture

Clean Architecture with four layers:

**Key patterns:**
- CQRS with MediatR — every operation is a Command or Query
- Repository pattern via IUnitOfWork
- Pipeline behaviours for cross-cutting concerns
- Domain-driven design — business rules live in domain entities

---

## Features

**Administrator Portal**
- JWT login with role-based authorization
- Card lifecycle management (Create, Activate, Block, Unblock, Suspend, Close)
- Wallet operations (Load, Debit, Credit, Balance Enquiry)
- Transaction management (Purchase, Reversal, Refund, Fee)
- Real-time fraud alert dashboard via SignalR
- Search across cards, transactions, merchants
- Report generation with CSV and JSON export
- Audit log viewer

**Cardholder Portal**
- View card balance and status
- View recent transaction history
- Receive notifications for card events

**Fraud Detection Engine**
Five independent rules evaluated on every purchase:
1. Large spend — R20,000+ within 10 minutes → Critical
2. International transaction → Medium
3. Rapid purchases — 3+ within 60 seconds → High
4. Multiple merchant categories within 1 minute → High
5. Excessive failed transactions — 5+ → High

**Notifications**
Mocked Email, SMS and Push notifications for:
Card Created, Blocked, Unblocked, Funds Loaded, Purchase Completed, Refund Processed, Low Balance, Fraud Alert

---

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (Express or full)
- Node.js 18+

### Backend

1. Clone the repository
```bash
git clone https://github.com/bhargavreddy2019/PayCentral.ExpenseCard.git
cd PayCentral.ExpenseCard
```

2. Update connection string in `PayCentral.WebApi/appsettings.json`:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=PayCentralExpenseCard;Trusted_Connection=True;TrustServerCertificate=True"
```

3. Run migrations and start:
```bash
cd src/PayCentral.WebApi
dotnet ef database update
dotnet run
```

4. Navigate to `https://localhost:7233/swagger`

### Frontend

```bash
cd paycentral-client
npm install
npm run dev
```

Navigate to `http://localhost:5173`

### Test Credentials
| Role | Email | Password |
|------|-------|----------|
| Administrator | admin@paycentral.co.za | Admin@123 |
| Cardholder | john.doe@paycentral.co.za | Cardholder@123 |
| Cardholder | jane.smith@paycentral.co.za | Cardholder@123 |

---

## Database Design

Key tables and relationships:
- **Users** → has many Cards, Notifications, AuditLogs
- **Cards** → has one Wallet, many Transactions, StatusHistory, FraudAlerts
- **Wallets** → optimistic concurrency via RowVersion
- **Transactions** → idempotency key prevents duplicate processing
- **FraudAlerts** → linked to Card and Transaction

Key indexes:
- `Transactions(CardId, CreatedAt DESC)` — most common query pattern
- `FraudAlerts(Severity, CreatedAt)` — admin dashboard sorting
- `Transactions(IdempotencyKey)` filtered unique — duplicate prevention

---

## Security

**Authentication & Authorization**
- JWT Bearer tokens with 15-minute expiry
- Refresh token rotation with 7-day expiry
- Role-based policies: AdminOnly, CardholderOnly, AdminOrCardholder
- BCrypt password hashing with work factor 12

**OWASP Top 10**
| Risk | Mitigation |
|------|-----------|
| A01 Broken Access Control | RBAC enforced at policy level on every endpoint |
| A02 Cryptographic Failures | BCrypt for passwords, HTTPS enforced, JWT signed with HMAC-SHA256 |
| A03 Injection | EF Core parameterized queries, FluentValidation on all inputs |
| A07 Identification Failures | JWT expiry, refresh token rotation, session invalidation |
| A09 Security Logging | Serilog structured logging, full audit trail on all mutations |

**POPIA Compliance**
- PII collected: name, email, phone number, transaction history
- Data minimisation — only collect what is operationally required
- Audit log on all data access and mutations
- Right to erasure — endpoint planned for future implementation
- Data breach notification obligation within 72 hours per POPIA Section 22

---

## API Endpoints

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| POST | /api/Auth/login | Login | Public |
| POST | /api/Auth/refresh | Refresh token | Public |
| GET | /api/Cards | List all cards | Admin |
| POST | /api/Cards | Create card | Admin |
| PUT | /api/Cards/{id}/block | Block card | Admin |
| PUT | /api/Cards/{id}/unblock | Unblock card | Admin |
| PUT | /api/Cards/{id}/suspend | Suspend card | Admin |
| PUT | /api/Cards/{id}/close | Close card | Admin |
| POST | /api/Transactions/load | Load funds | Admin |
| POST | /api/Transactions/purchase | Make purchase | Both |
| POST | /api/Transactions/refund | Process refund | Admin |
| POST | /api/Transactions/reversal | Reverse transaction | Admin |
| GET | /api/Fraud/alerts | Get fraud alerts | Admin |
| PUT | /api/Fraud/alerts/{id}/resolve | Resolve alert | Admin |
| GET | /api/Reports/transactions | Transaction report | Admin |
| GET | /api/Reports/fraud | Fraud report | Admin |
| GET | /api/Reports/cards | Card report | Admin |
| GET | /api/Reports/daily-summary | Daily summary | Admin |
| GET | /api/AuditLogs | View audit logs | Admin |
| GET | /health | Health check | Public |

---

## Testing

18 unit tests covering:
- Fraud rule engine (international, large spend, rapid purchases)
- Wallet balance validation (sufficient funds, zero amount, negative amount)
- Card status transitions (closed is terminal, pending cannot be blocked)
- Card domain methods (mask card number, generate card number)

```bash
cd tests/PayCentral.Tests
dotnet test
```

---

## AI Usage

See `AI-PROMPT-LOG.md` for full documentation of AI tool usage, prompts given, what was produced, what was changed manually, and engineering decisions made.

---

## Assumptions

- Card numbers are system-generated (Visa format starting with 4)
- International transaction = merchant country code != ZA
- Low balance threshold = R100 (notification trigger)
- Notification delivery is mocked — no real Email/SMS/Push provider integrated
- Currency is ZAR only — multi-currency is a future improvement
- Card number is stored as plain text — PCI-DSS tokenisation is a future improvement

---

## Future Improvements

- **Idempotency store in Redis** — currently database-based, Redis would be faster
- **Azure Service Bus** — move fraud detection and notifications to async queue
- **PCI-DSS tokenisation** — replace card numbers with tokens
- **OpenTelemetry** — distributed tracing across all operations
- **Right to erasure endpoint** — POPIA compliance
- **Multi-currency support** — currently ZAR only
- **Docker + Kubernetes** — containerisation and orchestration
- **Automated integration tests** — end-to-end card lifecycle testing
- **Rate limiting** — prevent brute force on auth endpoints
- **Azure Key Vault** — move secrets out of appsettings

---

## Diagrams

Architecture, ER and API Flow diagrams are included in the `/docs` folder.

---

*Built with Clean Architecture, CQRS, and engineering judgement over feature count.*
*PayCentral Senior Full Stack Developer Assessment — Bhargav*