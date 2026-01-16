# Project Context: MARBLIN
**Type:** Luxury Marble & Stone E-Shop (MVP)
**Architecture:** MVC (.NET 8 Web Application)
**Database:** SQL Server (Code-First Entity Framework)

## 1. Core Constraints (From SRS)
- **Guest Checkout Only:** Users do not create accounts. [cite_start]We identify them by Email + Order ID[cite: 16].
- [cite_start]**Payment Model:** - Deposit System: Users pay a % upfront (Configurable)[cite: 68, 69].
    - Method: Manual Transfer (Instapay/Vodafone Cash). [cite_start]Users upload a receipt image or Transaction ID[cite: 83, 84].
    - No Payment Gateway integration for MVP.
- [cite_start]**Admin:** Single admin user who manages inventory and verifies payments[cite: 30].

## 2. Key Entities (Domain)
- **Product:** Has Variants (Material, Size) and Images. [cite_start]Can be "Signature Piece"[cite: 57, 126].
- [cite_start]**Order:** Tracks status (Pending Deposit -> In Production -> Awaiting Balance -> Shipped)[cite: 111].
- [cite_start]**CustomRequest:** A form for users to request bespoke items[cite: 87].
