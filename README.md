# Grocery Manager

Grocery Manager makes the next grocery shop easy:

1. Add the regular items you use and where you store them.
2. Do a quick stocktake by stock area.
3. Generate a list using expected use before the next shop and each item’s buffer.
4. Check items off while shopping.

It deliberately does not try to be a detailed inventory-management system. Stock areas make counting practical; the next stocktake is the trusted view of what is on hand.

## Stack

- `web/` — Next.js, React, Tailwind, and shadcn-style components.
- `api/` — ASP.NET Core, EF Core, PostgreSQL, and ASP.NET Identity.

## Run locally

1. Create `api/src/GroceryManager.Api/appsettings.Development.json` from [`appsettings.example.json`](api/src/GroceryManager.Api/appsettings.example.json) and provide your local PostgreSQL connection string.
2. Apply the database migrations:

   ```bash
   cd api
   dotnet ef database update --project src/GroceryManager.Api/GroceryManager.Api.csproj
   ```

3. Start the API:

   ```bash
   cd api
   dotnet run --project src/GroceryManager.Api/GroceryManager.Api.csproj
   ```

   The API runs at `http://localhost:5080`.

4. In another terminal, start the web client:

   ```bash
   cd web
   pnpm install
   pnpm dev
   ```

   Open `http://localhost:3000`.

## Checks

```bash
cd api
dotnet build src/GroceryManager.Api/GroceryManager.Api.csproj --no-restore

cd ../web
pnpm typecheck
pnpm lint
pnpm test
```

## API client

The web client’s typed API client is generated from the running API OpenAPI document:

```bash
cd web
pnpm api:generate
```

Start the API first, as generation reads `http://localhost:5080/openapi/v1.json`.
