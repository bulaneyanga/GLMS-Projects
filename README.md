# Global Logistics Management System (GLMS)

A monolithic ASP.NET Core MVC prototype for TechMove Logistics. It manages clients, contracts, signed PDF agreements, and service requests with server-side USD-to-ZAR conversion.

## Technology

- ASP.NET Core MVC on .NET 10
- Entity Framework Core + SQL Server / Azure SQL Database
- Optional Azure Blob Storage for private agreement files (local filesystem by default)
- Async `HttpClient` integration with the free Open ER API (`USD` rates)
- Separate xUnit test project

> Azure Storage is not a relational database. Use **Azure SQL Database** for EF Core relational data and **Azure Blob Storage** for PDF files. The project can also use local SQL Server LocalDB and local disk for development.

## Run locally

1. Install the .NET 10 SDK and SQL Server LocalDB (or provide another SQL Server connection string).
2. In a terminal at the solution root, restore and build with `dotnet restore` and `dotnet build`.
3. Install the EF command-line tool if needed: `dotnet tool install --global dotnet-ef`.
4. Create and apply the database migration: `dotnet ef migrations add InitialCreate --project GLMS.csproj` then `dotnet ef database update --project GLMS.csproj`.
5. Start the site with `dotnet run --project GLMS.csproj`; browse to the local address printed by the command.
6. Run automated tests with `dotnet test GLMS.slnx`.

The initial LocalDB connection string is in `appsettings.json`. For a different SQL Server, set `ConnectionStrings__DefaultConnection` in the environment or configure .NET User Secrets. Do not commit credentials.

The generated EF migration is in `Migrations/`, with a deployable SQL Server script at `Migrations/InitialCreate.sql` for controlled/manual deployment.

## Azure configuration

Create an Azure SQL Database and set `ConnectionStrings__DefaultConnection` to its ADO.NET connection string. For contract files, create a private Blob container and configure:

- `Storage__Provider=AzureBlob`
- `Storage__BlobConnectionString` to the storage account connection string (keep this secret out of source control)
- `Storage__BlobContainer=signed-agreements`

The application creates the private container on first upload. Leave `Storage__Provider=Local` for local filesystem storage under `App_Data/agreements`. Database migration-on-startup is disabled by default; enable `Database__ApplyMigrations=true` for a controlled deployment that should apply reviewed pending migrations at startup (the Compose sample enables it).

The currency page fetches the current USD/ZAR quote asynchronously from `https://open.er-api.com/v6/latest/USD`. The browser previews the converted value; the server fetches the quote again and independently calculates the ZAR value when saving, so the posted preview cannot alter the persisted conversion.

## Docker

Set a strong `MSSQL_SA_PASSWORD` environment variable before starting the Compose stack. The provided Compose configuration runs the MVC app with a SQL Server container and a persistent local agreement volume, applying EF migrations at startup. To use Azure SQL instead, set the app's connection string and remove/disable the `sqlserver` service in your deployment configuration. For production, use managed identity/Key Vault and Azure Blob Storage rather than SQL passwords or local file storage.

## Business rules and evidence

- A service request is blocked for `Expired` and `On Hold` contracts; Draft and Active contracts are eligible.
- Contract agreement uploads are required on creation and must have a `.pdf` extension and a `%PDF-` file signature. Maximum upload size is 20 MB.
- Contract search supports status and date-overlap filters.
- Tests cover currency rounding, restricted upload extensions and contract eligibility. Capture the requested Test Explorer screenshot in Visual Studio Code after running the test project; screenshots are not generated automatically by the source code.
- This is an assignment prototype and currently has no sign-in or role authorization. Add ASP.NET Core Identity or Microsoft Entra ID authorization before exposing it to real administrators or customers.

## Database changes

EF Core migration files are generated in the `Migrations` directory by the documented `dotnet ef migrations add` command. Review generated migrations before applying to shared or production databases.
