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