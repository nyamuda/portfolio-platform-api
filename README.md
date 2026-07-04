# Portfolio Platform API

This is the backend for the Portfolio Platform.

The API uses a layered backend structure:

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT authentication
- DTO-based request and response contracts
- controller -> service abstraction -> service implementation flow
- clear folders for models, DTOs, services, controllers, helpers, and data access

## Runtime

This project targets .NET 10.

If local restore/build fails with an SDK error, install the .NET 10 SDK first.

## First domains

The first scaffold includes:

- Auth
- Users
- Profile setup
- Public profile lookup
- Contact messages

Projects, case studies, blog posts, testimonials, media uploads, and admin tooling will build on this foundation.

