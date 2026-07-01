# AI Development Log — PayCentral Expense Card Platform

**Developer:** Bhargav  
**Assessment:** PayCentral Senior Full Stack Developer  
**AI Tool Used:** Claude (Anthropic)  
**Started:** [Today's date + time]

---

## How This Log Works
Each entry captures:
- What I asked AI
- What AI produced
- What I changed manually and why
- My own engineering decisions

---

## Entry 1 — Project Planning & Architecture

**Prompt:**
"I have a 48 hour technical assessment for a fintech company 
building a prepaid expense card platform. I chose the Backend 
Focus option. What should my Clean Architecture solution 
structure look like and what NuGet packages do I need?"

**AI Produced:**
- 48-hour execution plan broken into phases
- Solution structure with 5 projects
- NuGet package list per layer
- Folder structure per project

**My Decisions:**
- Chose React over Angular (existing familiarity)
- Chose to include Docker and SignalR as bonus features
- Decided to build a data seeder for smooth demo experience
- Prioritised backend depth per assessment weighting 
  (Architecture 20%, API 15%, DB 15%)

**What AI Missed:**
- Nothing at planning stage — architectural decisions 
  were mine to make

---

## Entry 2 — Solution Scaffold

**Prompt:**
"I'm building a prepaid expense card platform in .NET 8 
using Clean Architecture. What projects should I create 
and how should they reference each other?"

**AI Produced:**
- Project creation steps via Visual Studio
- Correct dependency direction (Domain → no dependencies)
- Folder structure per project
- NuGet packages per layer

**My Decisions:**
- Kept Domain completely free of any NuGet packages
- Added BCrypt.Net for password hashing
- Added FluentAssertions to Tests for readable test output

**What I Changed:**
- Nothing structural — scaffold is standard Clean Architecture

---

## Entry 3 — Domain Entities & Interfaces

**Prompt:**
"I'm building the domain layer for a prepaid expense card 
platform. What entities do I need and what properties 
should each have? I need Cards, Wallets, Transactions, 
Fraud Alerts, Notifications and Audit Logs."

**AI Produced:**
- 10 entity classes with correct relationships
- 7 enums covering all status and type values
- Repository interfaces following generic pattern
- IUnitOfWork interface

**My Decisions:**
- Added RowVersion to Wallet for optimistic concurrency — 
  AI missed this, critical for preventing negative balances
- Added CanTransitionTo() on Card — closed cards are terminal,
  AI had no awareness of this fintech domain rule
- Added CanDebit() on Wallet — domain logic stays in domain,
  not in application handlers
- Added IdempotencyKey on Transaction — prevents duplicate 
  payment processing, AI did not include this initially
- Added GenerateCardNumber() and GenerateReference() as 
  static domain methods — business logic belongs in domain

**What AI Missed:**
- Optimistic concurrency on Wallet (RowVersion)
- Idempotency key on Transaction
- Domain transition rules on Card
- MaskedCardNumber for PCI-DSS compliance


## Entry 4 — DbContext & EF Core Configurations

**Prompt:**
"I have these domain entities for a prepaid card platform. 
How should I configure them in EF Core using 
IEntityTypeConfiguration? I need proper indexes, 
decimal precision and relationships."

**AI Produced:**
- AppDbContext with all DbSets
- Configuration class per entity
- Relationships, constraints and indexes
- AppDbContextFactory for design time migrations

**My Decisions:**
- Added composite index on Transactions (CardId, CreatedAt)
  for most common query pattern
- Added composite index on FraudAlerts (Severity, CreatedAt)
  for admin dashboard
- Added filtered unique index on IdempotencyKey
- Pinned all EF Core packages to version 8.0.11 — 
  NuGet was pulling v10 which is incompatible with .NET 8

**What AI Missed:**
- Design time factory needed for migrations to work
- Package version conflicts between EF Core 10 and .NET 8
- Missing configuration packages for SetBasePath


## Entry 6 — Data Seeder

**Prompt:**
"I need a data seeder for my prepaid card platform. 
I need an admin user, two cardholders, some merchants, 
cards with wallets and sample transactions including 
one that should trigger a fraud alert."

**AI Produced:**
- Static DataSeeder class with idempotent seed methods
- 3 users, 3 merchants, 2 cards with wallets, 5 transactions
- One international transaction and one large spend 
  to trigger fraud rules later

**My Decisions:**
- Used fixed GUIDs for seed data — makes foreign key 
  relationships predictable and testable
- Included a R21,000 transaction to pre-trigger the 
  fraud rule engine we build next
- BCrypt work factor 12 consistent with production setting

**What AI Missed:**
- Nothing — seeder is straightforward


## Entry 8 — Wallet & Transactions

**Prompt:**
"I need wallet operations for a prepaid card platform. 
I need Load Funds, Purchase, Refund and Reversal. 
It must prevent negative balances, block transactions 
on blocked cards and prevent duplicate requests using 
idempotency keys."

**AI Produced:**
- LoadFunds, Purchase, Refund, Reversal handlers
- GetBalance and GetTransactions query handlers
- TransactionsController with role-based endpoints
- FluentValidation on all commands

**My Decisions:**
- Idempotency check on every mutating operation
- CanDebit() domain method enforces negative balance rule
- Reversal marks original transaction as Reversed status
- Blocked and Suspended cards both decline purchases
- Closed cards decline all operations

**What AI Missed:**
- Nothing significant

## Entry 9 — Fraud Detection Engine

**Prompt:**
"I need a fraud rule engine for a prepaid card platform. 
Each rule should be independent and easy to add new ones. 
Rules needed: large spend in 10 minutes, international 
transaction, rapid purchases, multiple merchant categories, 
excessive failed transactions."

**AI Produced:**
- IFraudRule interface with FraudRuleResult
- 5 independent rule implementations
- FraudService that evaluates all rules per transaction
- SignalR hub for real-time admin alerts
- FraudController with get and resolve endpoints

**My Decisions:**
- Used IEnumerable<IFraudRule> injection pattern so new 
  rules can be added by registering in DI only
- Fraud detection runs AFTER transaction saves — 
  fraud check never blocks a legitimate transaction
- Moved IFraudHubService to Application interfaces — 
  keeps Infrastructure free of WebApi dependency

**What AI Missed:**
- Circular dependency — Infrastructure cannot reference 
  WebApi. Fixed by using base Hub type in FraudHubService