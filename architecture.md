# PayCentral — Architecture Diagrams

## 1. Solution Architecture

```mermaid
graph TB
    subgraph Client["Frontend (React + TypeScript)"]
        UI[Login / Admin Dashboard / Cardholder Portal]
        SR[SignalR Client]
    end

    subgraph WebApi["PayCentral.WebApi"]
        CTR[Controllers]
        MW[Exception Middleware]
        HUB[FraudHub SignalR]
        HC[Health Checks]
    end

    subgraph Application["PayCentral.Application"]
        CMD[Commands]
        QRY[Queries]
        HDL[MediatR Handlers]
        VAL[FluentValidation]
        INT[Interfaces]
    end

    subgraph Infrastructure["PayCentral.Infrastructure"]
        EF[EF Core / AppDbContext]
        JWT[JWT Service]
        PWD[Password Service]
        FRD[Fraud Service]
        NTF[Notification Service]
        AUD[Audit Service]
        CSV[CSV Export Service]
    end

    subgraph Domain["PayCentral.Domain"]
        ENT[Entities]
        ENM[Enums]
        IRP[Repository Interfaces]
    end

    DB[(SQL Server)]

    UI -->|HTTP + Bearer Token| CTR
    SR <-->|WebSocket| HUB
    CTR --> MW
    CTR --> HDL
    HDL --> CMD
    HDL --> QRY
    CMD --> VAL
    HDL --> EF
    HDL --> FRD
    HDL --> NTF
    HDL --> AUD
    EF --> DB
    FRD --> HUB
    Application --> Domain
    Infrastructure --> Application
    WebApi --> Application
    WebApi --> Infrastructure
```

---

## 2. Entity Relationship Diagram

```mermaid
erDiagram
    Users {
        Guid Id PK
        string FirstName
        string LastName
        string Email
        string PasswordHash
        string PhoneNumber
        UserRole Role
        bool IsActive
        string RefreshToken
        DateTime RefreshTokenExpiry
    }

    Cards {
        Guid Id PK
        string CardNumber
        string MaskedCardNumber
        Guid UserId FK
        CardStatus Status
        DateTime ExpiryDate
        string BlockReason
        DateTime BlockedAt
        DateTime ActivatedAt
        DateTime ClosedAt
    }

    Wallets {
        Guid Id PK
        Guid CardId FK
        decimal Balance
        decimal AvailableBalance
        string Currency
        byte[] RowVersion
    }

    Transactions {
        Guid Id PK
        string ReferenceNumber
        Guid CardId FK
        Guid MerchantId FK
        TransactionType Type
        TransactionStatus Status
        decimal Amount
        decimal BalanceAfter
        string Currency
        bool IsInternational
        string IdempotencyKey
    }

    Merchants {
        Guid Id PK
        string Name
        string Category
        string CountryCode
        string City
    }

    CardStatusHistory {
        Guid Id PK
        Guid CardId FK
        CardStatus FromStatus
        CardStatus ToStatus
        string Reason
        string ChangedBy
    }

    FraudAlerts {
        Guid Id PK
        Guid CardId FK
        Guid TransactionId FK
        string AlertType
        string Reason
        FraudSeverity Severity
        bool IsResolved
        DateTime ResolvedAt
    }

    Notifications {
        Guid Id PK
        Guid UserId FK
        NotificationType Type
        NotificationChannel Channel
        string Title
        string Message
        bool IsRead
        bool IsSent
    }

    AuditLogs {
        Guid Id PK
        Guid UserId FK
        string Action
        string EntityName
        string EntityId
        string OldValues
        string NewValues
        string IpAddress
        bool IsSuccess
    }

    Users ||--o{ Cards : "has"
    Users ||--o{ Notifications : "receives"
    Users ||--o{ AuditLogs : "generates"
    Cards ||--|| Wallets : "has one"
    Cards ||--o{ Transactions : "has"
    Cards ||--o{ CardStatusHistory : "tracks"
    Cards ||--o{ FraudAlerts : "triggers"
    Merchants ||--o{ Transactions : "processes"
    Transactions ||--o{ FraudAlerts : "may trigger"
```

---

## 3. API Flow Diagram — Purchase Transaction

```mermaid
sequenceDiagram
    participant C as Client
    participant API as WebApi
    participant MED as MediatR
    participant HDL as PurchaseHandler
    participant DB as SQL Server
    participant FRD as FraudEngine
    participant HUB as SignalR Hub
    participant ADM as Admin Dashboard

    C->>API: POST /api/Transactions/purchase (Bearer Token)
    API->>API: JWT Middleware validates token
    API->>API: ExceptionMiddleware wraps request
    API->>MED: mediator.Send(PurchaseCommand)
    MED->>MED: FluentValidation pipeline
    MED->>HDL: Handle(PurchaseCommand)
    HDL->>DB: Check idempotency key
    HDL->>DB: Load card + wallet
    HDL->>HDL: Validate card status (not blocked/closed)
    HDL->>HDL: wallet.CanDebit(amount)
    HDL->>DB: Debit wallet balance
    HDL->>DB: Save transaction
    HDL->>FRD: EvaluateTransactionAsync()
    FRD->>FRD: Run all 5 fraud rules
    FRD->>DB: Save FraudAlert (if triggered)
    FRD->>HUB: SendFraudAlertAsync()
    HUB->>ADM: ReceiveFraudAlert (real-time)
    HDL-->>API: TransactionDto
    API-->>C: 200 OK ApiResponse<TransactionDto>
```

---

## 4. Card Lifecycle State Machine

```mermaid
stateDiagram-v2
    [*] --> Pending : Card Created
    Pending --> Active : Activate
    Active --> Blocked : Block
    Active --> Suspended : Suspend
    Active --> Closed : Close
    Blocked --> Active : Unblock
    Blocked --> Closed : Close
    Suspended --> Active : Activate
    Suspended --> Blocked : Block
    Suspended --> Closed : Close
    Closed --> [*] : Terminal State
```