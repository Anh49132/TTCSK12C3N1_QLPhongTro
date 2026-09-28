# Staging demo

## Setup

Requires Docker Desktop with Docker Compose and a .NET 10 SDK only for local non-container runs. From the repository root:

```powershell
Copy-Item .env.example .env
```

Replace both placeholder values in `.env` with unique secrets. `STAGING_DEMO_PASSWORD` is shared by the four demo accounts. `STAGING_JWT_SECRET` must be at least 32 characters. `.env` is ignored by Git.

## Seed and start

The reset command only runs with `ASPNETCORE_ENVIRONMENT=Staging`, an explicit staging database path, and `Staging__AllowReset=true`. Compose sets the database to the named `staging-data` volume; the development SQLite file is neither mounted nor included in the Docker build context.

```powershell
docker compose run --rm --no-deps app --reset-staging-demo
docker compose up -d app
```

For a later reset, stop the web process first so SQLite is not open, then run the seed command and start the app again:

```powershell
docker compose stop app
docker compose run --rm --no-deps app --reset-staging-demo
docker compose up -d app
```

Open `http://localhost:8080` (or the configured `STAGING_PORT`), sign in, then choose **Staging** in the header. Demo usernames are `demo-khach_thue@staging.test`, `demo-chu_nha@staging.test`, `demo-quan_ly@staging.test`, and `demo-admin@staging.test`; all use `STAGING_DEMO_PASSWORD`.

The seed command refuses untagged existing SQLite files, rejects unsupported staging schema versions, creates a versioned staging-only schema on a new empty database, then transactionally replaces its sample rows. It preserves the permission matrix and recreates current demo accounts, profiles, buildings, rooms, contracts, invoice terms, invoices, invoice lines, and payment records. It does not touch the development SQLite database.

The synthetic dataset has 2 buildings, 30 rooms, 20 active contracts, 60 invoices across 3 periods, and exactly 20 invoices in each disjoint state: paid in full, partially paid but not yet due, and overdue with no payment. Contract/invoice dates are based on the reset date so the sample remains usable over time. Each reset validates counts before commit and reports elapsed time.

## Verification and limits

- Seeded a disposable SQLite staging file twice. Both runs passed exact count/state assertions; measured reset time was 4.37 seconds for the first run including schema setup and 0.80 seconds for the repeat reset.
- `StagingSeedAllowsDemoAdminToViewVerifiedSummary` passed, including a stale-account removal check, demo admin login, and dashboard values.
- All 8 tests in the `PermissionTests` class passed: the original seven permission tests plus the staging integration test. Coverage includes all 36 role/module pairs and direct HTTP 403 checks for denied module and room-write routes.
- The repository does not implement the contract, invoice, payment, or remaining module workflows yet. The staging-only schema supports sample records and summary display; it is not a production schema migration and does not make those placeholder modules operational. A complete business end-to-end demo is not verified.
- Docker is unavailable in the current environment, so Docker Compose build/start and visual browser checks remain unverified. The dashboard was verified through the application integration test.
- The local development SQLite file was not read or changed during this task. Existing unrelated SQLite/test-project/backup changes in the worktree were preserved.