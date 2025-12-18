# Provenance Tracker API

API for tracking product provenance as a lightweight, hash-chained ledger. Features JWT auth with Identity roles (Admin, User, Participant), transaction approval, and integrity verification.

## How It Works
- Identity-backed auth with JWT: tokens issued via login endpoints and validated with configured issuer/audience/key in `appsettings.json`.
- Roles: `Admin` manages approvals, `User` can request Participant upgrade, `Participant` submits transactions.
- Hash-chained transactions per product: each confirmed transaction references the previous hash; integrity checks run before confirmations to detect tampering.
- Pending/confirmed/cancelled workflow: participants create pending transactions, admins confirm or cancel them.

## Project Layout
- Startup & DI: [Program.cs](Program.cs) configures controllers, Swagger, CORS, Identity, JWT, and seeds roles/admin.
- Data access: [Data/ApplicationDbContext.cs](Data/ApplicationDbContext.cs) exposes `Products` and `Transactions` DbSets on top of Identity.
- Domain models: [Models/Entities/Product.cs](Models/Entities/Product.cs), [Models/Entities/Transaction.cs](Models/Entities/Transaction.cs), [Models/Entities/ApplicationUser.cs](Models/Entities/ApplicationUser.cs#L8) (adds `IsApproved`).
- DTOs: [Models/AuthDto.cs](Models/AuthDto.cs), [Models/CreateTransactionDto.cs](Models/CreateTransactionDto.cs).
- Services: [Services/HashTransaction.cs](Services/HashTransaction.cs) computes SHA-256; [Services/VerifyTransactions.cs](Services/VerifyTransactions.cs) replays the chain to ensure hashes link correctly.
- Controllers: auth ([Controllers/AuthController.cs](Controllers/AuthController.cs)), participant flows ([Controllers/ParticipantController.cs](Controllers/ParticipantController.cs)), admin approvals ([Controllers/AdminController.cs](Controllers/AdminController.cs)), transaction queries ([Controllers/TransactionController.cs](Controllers/TransactionController.cs)), and role upgrade ([Controllers/UserController.cs](Controllers/UserController.cs)).

## Data Model
- Product: `Id`, `Status`, `Description`.
- Transaction: `Id`, `ProductId`, `ParticipantId`, `Description`, `Status`, `Location`, `ConfirmationStatus`, `PreviousHash`, `CurrentHash`, `CreatedAt`.
- ApplicationUser: Identity user with `IsApproved` flag to gate participant actions.

## Hash Chain Logic
For each confirmed transaction, the block payload is:

```
{ProductId}{ParticipantId}{Status}{CreatedAt.Ticks}{PreviousHash}{Location}{Description}
```

The SHA-256 of this payload becomes `CurrentHash`. A chain is valid iff the first `PreviousHash` is 64 zeros and every subsequent `PreviousHash` matches the prior `CurrentHash`. `VerifyTransactions.VerifyChainAsync(productId)` enforces this before admin confirmation.

## API Endpoints (happy-path summary)
### Auth
- `POST /api/auth/register` — create user with role `User`.
- `POST /api/auth/login` — issue JWT including roles and `IsApproved`.
- `POST /api/auth/external-login` — upsert external user, return JWT.

### Users
- `PUT /api/user/participant-approve` (role: User) — add `Participant` role to caller.

### Participants
- `POST /api/participant` (role: Participant; user must be approved) — create pending transaction; creates product on first transaction; hashes link to last confirmed transaction.
- `GET /api/participant` (role: Participant) — list caller’s transactions.

### Admins
- `GET /api/admin/pending-transactions` (role: Admin) — list pending transactions.
- `POST /api/admin/confirm-transaction/{id}` (role: Admin) — verify chain then mark transaction confirmed.
- `POST /api/admin/cancel-transaction/{id}` (role: Admin) — cancel pending transaction.
- `GET /api/admin/pending-users` (role: Admin) — list participants awaiting approval (`IsApproved == false`).
- `POST /api/admin/confirm-user/{email}` (role: Admin) — mark user approved.

### Transactions (public)
- `GET /api/transaction/{productId}` — return confirmed chain for a product plus `chainValid` flag.

## Identity & Seeding
- Roles `Admin`, `User`, `Participant` are seeded at startup.
- Default admin: email `moustafa1moon@gmail.com`, password `123456` (edit in [Program.cs](Program.cs#L61-L84)).

## Running Locally
1) Update the SQL Server connection string in [appsettings.json](appsettings.json#L2-L4) if needed.
2) Apply migrations if the database is empty:
   ```bash
   dotnet ef database update
   ```
3) Run the API:
   ```bash
   dotnet run --project provenancetracker.csproj
   ```
4) Swagger UI available at `/swagger` in Development.

## Auth Configuration
- JWT issuer/audience/key pulled from [appsettings.json](appsettings.json#L5-L11).
- CORS policy `AllowNextjs` allows `http://localhost:3000` with any headers/methods.

## Approval & Workflow Notes
- Users must request participant role, then an admin sets `IsApproved` before they can post transactions.
- Admin confirmation enforces chain integrity; if verification fails, confirmation is rejected.
- Participants can still see their own transactions even if pending.

## Extending
- Add more product metadata in `Product` and include in hash payload to bind it to the chain.
- Add pagination/filters on list endpoints to control payload sizes.
- Consider auditing admin actions (who confirmed/cancelled) and storing reasons.
