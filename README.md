# Portfolio Platform API

The Portfolio Platform API powers authentication, owner-managed portfolio content, public portfolio delivery, classification data, contact email, dashboard summaries, and portfolio design selection.

This repository is the backend half of the platform. The Vue frontend lives in `portfolio-platform-frontend`.

## Current status

Implemented domains include:

- email registration, login, refresh tokens, email verification, and password reset;
- Google OAuth sign-up and sign-in;
- users and public profiles;
- projects and reusable tags;
- blog posts and managed topics;
- offerings;
- experience timeline entries and experience types;
- dashboard summaries;
- public contact email delivery;
- profile theme, accent colour, and typography selection;
- idempotent seed data for common topics, tags, and experience types.

Contact messages are delivered through SMTP and are not stored as database records. Images are uploaded by the frontend to Supabase Storage; the API stores image URLs only.

## Technology

- .NET 10 ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL through Npgsql
- JWT bearer authentication
- Google OAuth
- MailKit for SMTP email
- first-party ASP.NET Core OpenAPI generation
- Scalar interactive API reference
- camel-case string enum serialization

## Architecture

The API follows a documented layered flow:

```text
Controller
  -> service abstraction
      -> service implementation
          -> ApplicationDbContext
              -> PostgreSQL
```

Responsibilities are separated as follows:

- **Controllers** define HTTP routes, authorization requirements, identity extraction, and response mapping.
- **Service abstractions** document the domain contract with XML comments.
- **Service implementations** enforce ownership, publication, relationship, slug, and update rules.
- **DTOs** define request and response contracts without exposing EF Core entities directly.
- **Models** define persisted data and straightforward scalar constraints.
- **ApplicationDbContext** defines relationship shape, join tables, delete behaviour, and seeder registration.
- **Seeders** add shared vocabulary idempotently through EF Core `UseSeeding` and `UseAsyncSeeding`.

Read `docs/Backend_Implementation_Guardrails.md` before adding or restructuring a feature.

## Repository structure

```text
Controllers/          HTTP endpoints
Data/                 DbContext and seeders
Dtos/                 request, response, filter, and pagination contracts
Enums/                API-aligned domain and query enums
Exceptions/           expected domain exception types
Helpers/              reusable backend helpers
Migrations/           EF Core schema history
Models/               persisted entities and configuration models
Services/
  Abstractions/       documented service contracts
  Implementations/    business and persistence workflows
```

## Prerequisites

- .NET 10 SDK
- access to a PostgreSQL database
- EF Core command-line tools compatible with EF Core 10
- SMTP credentials for account email and contact delivery
- Google OAuth credentials when testing OAuth flows

Check installed versions with:

```powershell
dotnet --version
dotnet ef --version
```

If the global EF tool is older than the project runtime, update it before generating migrations:

```powershell
dotnet tool update --global dotnet-ef
```

## Configuration

Local secrets should be stored with .NET user secrets or environment variables. Do not commit real database passwords, JWT keys, SMTP passwords, or OAuth secrets.

Required configuration paths include:

```text
ConnectionStrings:DefaultConnection
Frontend:LocalUrl
Frontend:ProductionUrl
Company:Name
Company:Email
Authentication:JwtSettings:Issuer
Authentication:JwtSettings:Audience
Authentication:JwtSettings:Key
Authentication:JwtSettings:ExpiresInMinutes
Authentication:OAuth:Google:ClientId
Authentication:OAuth:Google:ClientSecret
Authentication:OAuth:Google:SigninRedirectUrl
Authentication:OAuth:Google:SignupRedirectUrl
Authentication:SmtpSettings:SenderEmail
Authentication:SmtpSettings:SenderName
Authentication:SmtpSettings:Password
Authentication:SmtpSettings:Host
```

Example user-secret commands:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Authentication:JwtSettings:Key" "use-a-long-random-development-key"
dotnet user-secrets set "Authentication:SmtpSettings:Password" "your-smtp-password"
dotnet user-secrets set "Authentication:OAuth:Google:ClientSecret" "your-google-client-secret"
```

The frontend development origin must match `Frontend:LocalUrl` so browser requests satisfy the configured CORS policy.

## Database setup

Restore packages, apply migrations, and run registered seeders with:

```powershell
dotnet restore
dotnet ef database update
```

The database seeders run through EF Core migration and database-initialization operations. Starting `dotnet watch` alone does not call `Migrate` or `EnsureCreated`, so it does not independently trigger seeding.

Create a migration after a real model/schema change:

```powershell
dotnet ef migrations add DescribeTheSchemaChange
dotnet ef database update
```

Review generated migrations before applying them. This project is still in development, but migrations should still describe the intended schema clearly.

## Run locally

```powershell
dotnet watch
```

The HTTP launch profile listens on:

```text
http://localhost:5218
```

Development launch profiles open Scalar at:

```text
http://localhost:5218/scalar/v1
```

The raw OpenAPI document is exposed in Development through the first-party OpenAPI middleware.

## API conventions

Owner-managed content generally follows this route shape:

```text
GET    /api/{resource}/me
GET    /api/{resource}/me/{id}
POST   /api/{resource}
PUT    /api/{resource}/{id}
DELETE /api/{resource}/{id}
```

Public content uses profile slugs and returns published records only:

```text
GET /api/{resource}/profile/{profileSlug}
```

Growing collections use filter DTOs and the shared paginated response/PageInfo contract. Public and owner projections should avoid returning full rich HTML where a list only needs summary data.

## Portfolio theme selection

The API stores three scalar values directly on `Profile`:

- `Theme`
- `ThemeAccentColor`
- `ThemeTypography`

Theme endpoints are:

```text
GET /api/profiles/me/theme
PUT /api/profiles/me/theme
GET /api/profiles/{profileSlug}/theme
```

The API persists choices; the frontend owns compiled Vue theme implementations. There is no database theme catalogue, design draft workflow, theme-management controller, or arbitrary JSON settings contract.

The authoritative theme documentation lives in the frontend repository:

```text
portfolio-platform-frontend/docs/Portfolio_Themes_User_Stories_SRS.md
portfolio-platform-frontend/docs/Portfolio_Themes_Technical_SRS.md
```

## Media boundary

The frontend uploads profile, project, post, and rich-content images directly to the `portfolio-platform` Supabase Storage bucket. DTOs sent to this API contain stored URLs, not image files or base64 data.

When an update removes hosted images, frontend cleanup runs only after the API update succeeds. The API must not assume responsibility for deleting storage objects unless the architecture is explicitly changed later.

## Documentation

Product-wide documentation is maintained in the frontend repository so user stories, design rules, public rendering, and cross-repository contracts remain together:

```text
portfolio-platform-frontend/docs/README.md
portfolio-platform-frontend/docs/Portfolio_Platform_User_Stories_SRS.md
portfolio-platform-frontend/docs/Portfolio_Platform_Technical_SRS.md
portfolio-platform-frontend/docs/Design.md
portfolio-platform-frontend/docs/Code_Documentation_Guide.md
```

Backend source contracts remain documented through XML comments and natural inner comments. Keep documentation and implementation synchronized whenever a route, DTO, relationship, or workflow changes.

## Verification

Run the backend build after each implementation change:

```powershell
dotnet build
```

For schema work, also run:

```powershell
dotnet ef database update
```

Exercise changed endpoints through Scalar or the frontend, including authorization, ownership, missing-record, validation, and public-publication cases.
